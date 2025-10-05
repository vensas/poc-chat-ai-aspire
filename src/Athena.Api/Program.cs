using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaApiFacade.DTOs;
using OllamaApiFacade.Extensions;
using Scalar.AspNetCore;
using Athena.Api.Database;
using Athena.Api.Extensions;
using Athena.Api.Plugins;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<AthenaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("postgres-default") 
        ?? throw new InvalidOperationException("Connection string 'postgres-default' not found.");
    options.UseNpgsql(connectionString);
});

var ollamaEndpoint = Environment.GetEnvironmentVariable("services__ollama__ollama__0")
    ?? throw new InvalidOperationException("Environment variable 'services__ollama__ollama__0' not found. Ensure Ollama service is running and the environment variable is set.");

builder.Services.AddKernel()
    .AddCustomChatCompletion("qwen3:1.7b", ollamaEndpoint)
    .Plugins.AddFromType<CustomerSalesPlugin>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();
    await dbContext.Database.MigrateAsync();
    
    // Seed sample data in development
    if (app.Environment.IsDevelopment())
    {
        await dbContext.SeedDataAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => "Welcome to the Athena API! Use the following endpoints: GET /api/customers, GET /api/sales-orders, or POST /api/chat for AI-powered analysis.");

// API endpoints for customers and sales orders
app.MapGet("/api/customers", async (AthenaDbContext dbContext) =>
{
    var customers = await dbContext.Customers
        .OrderBy(x => x.Name)
        .ToListAsync();
    return customers;
});

app.MapGet("/api/customers/{customerId}", async (string customerId, AthenaDbContext dbContext) =>
{
    var customer = await dbContext.Customers
        .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
});

app.MapGet("/api/sales-orders", async (AthenaDbContext dbContext) =>
{
    var salesOrders = await dbContext.SalesOrders
        .OrderByDescending(x => x.OrderDate)
        .ToListAsync();
    return salesOrders;
});

app.MapGet("/api/sales-orders/customer/{customerId}", async (string customerId, AthenaDbContext dbContext) =>
{
    var salesOrders = await dbContext.SalesOrders
        .Where(s => s.CustomerId == customerId)
        .OrderByDescending(x => x.OrderDate)
        .ToListAsync();
    return salesOrders;
});

// Let's act as an Ollama chat backend
app.MapOllamaBackendFacade();
// The main chat endpoint, called by open-webui
app.MapPost("/api/chat", async (HttpContext context, IChatCompletionService chatCompletionService, Kernel kernel) =>
{

    var chatRequest = await context.Request.ReadFromJsonAsync<ChatRequest>();
    if (chatRequest == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid request body");
        return;
    }

     // Debug: Check if plugins are loaded
    var plugins = kernel.Plugins;
    Console.WriteLine($"Loaded plugins: {plugins.Count}");
    foreach (var plugin in plugins)
    {
        Console.WriteLine($"Plugin: {plugin.Name}, Functions: {plugin?.Count()}");
    }

    
    var chatHistory = chatRequest.ToChatHistory();
    chatHistory.AddSystemMessage("""
When asked for customer, sales, or order data, use the available tools:
- GetSalesForCustomer(customerId) for sales orders of a specific customer
- GetAllSales() for all sales orders
- GetCustomer(customerId) for customer information
- GetAllCustomers() for all customers

Provide analysis from the perspective of a sales analyst or dispatcher who 
needs to understand customer purchase patterns and make recommendations 
based on sales data and customer behavior.
""");

    var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
    };

    try
    {
        await chatCompletionService
            .GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel)
            .StreamToResponseAsync(context.Response);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
});

await app.RunAsync();

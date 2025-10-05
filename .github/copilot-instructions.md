# Athena AI Sales Analysis - Copilot Instructions

## Project Overview
This is a .NET Aspire-orchestrated AI sales analysis system that combines SemanticKernel with Ollama LLM for function calling on PostgreSQL customer/sales data. The system uses a facade pattern where the Athena API serves both business logic AND acts as an OpenAI-compatible proxy to Ollama.

## Architecture & Service Dependencies
- **Athena.Api**: ASP.NET Core 9 + SemanticKernel + EF Core with PostgreSQL
- **Athena.AppHost**: .NET Aspire orchestrator managing all services
- **Dependencies**: PostgreSQL → Ollama (qwen3:1.7b) → Open-WebUI
- **Key Pattern**: API Facade - Athena API proxies Ollama using `OllamaApiFacade` NuGet package

## Critical Development Workflows

### Running the Application
```bash
# Primary method: Use VS Code F5 with "Athena AppHost (HTTP)" profile
# Or manually:
cd src/Athena.AppHost
dotnet run --launch-profile http
```
- Aspire dashboard auto-opens at http://localhost:15230
- Wait for Ollama model pull (first run only) - check container logs
- Open-WebUI available at http://localhost:8181

### Entity Framework Operations
```bash
# REQUIRED: Set connection string environment variable first
export ConnectionStrings__postgres-default="Host=localhost;Database=athena;Username=postgres;Password=postgres"

# Then run EF commands
dotnet ef migrations add YourMigrationName --project src/Athena.Api
dotnet ef database update --project src/Athena.Api
```

## Key Patterns & Conventions

### SemanticKernel Plugin Pattern
All AI functions use dependency injection with scoped DbContext:
```csharp
public class CustomerSalesPlugin(IServiceProvider serviceProvider)
{
    [KernelFunction]
    [Description("Clear description for LLM")]
    public async Task<Result> FunctionName([Description("Param description")] string param)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();
        // Implementation
    }
}
```

### Database Schema Convention
- Tables use `athena` schema with snake_case names (`customers`, `sales_orders`)
- Entities use Pascal case (`CustomerEntity`, `SalesOrderEntity`)
- Always include indexed fields: `CustomerId`, `SalesOrderId`, `OrderDate`

### Ollama Integration Pattern
- Use `KernelBuilderExtensions.AddCustomChatCompletion()` for OpenAI-compatible Ollama client
- Environment variable: `services__ollama__ollama__0` (set by Aspire)
- Model: `qwen3:1.7b` (supports function calling, optimized for size/performance)

### API Facade Pattern
```csharp
// Business endpoints
app.MapGet("/api/customers", async (AthenaDbContext dbContext) => { });

// Ollama proxy (OpenAI-compatible)
app.MapOllamaBackendFacade(); // From OllamaApiFacade package

// Chat endpoint with SemanticKernel
app.MapPost("/api/chat", async (IChatCompletionService chatService, Kernel kernel) => {
    // Always use FunctionChoiceBehavior.Required() for reliable function calling
});
```

### Container & Aspire Patterns
- All services use persistent volumes and wait dependencies in `AppHost.cs`
- Ollama requires custom startup script (`ollama-startup.sh`) for model initialization
- Open-WebUI connects through API facade, not directly to Ollama

## Common Pitfalls
- EF migrations fail without connection string environment variable
- Ollama model pull can take 5-10 minutes on first run - check container logs
- Function calling requires `FunctionChoiceBehavior.Required()` for reliability
- Database seeding only runs in Development environment

## Testing & Debugging
- Use Aspire dashboard to monitor service health and logs
- Test AI functions via Open-WebUI: "Show me all customers" or "Analyze sales for CUST-001"
- API documentation available at `/scalar` endpoint during development
- Check Ollama model status: container logs show "Available models:" output
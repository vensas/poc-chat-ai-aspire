using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using Athena.Api.Database;

namespace Athena.Api.Plugins;

public class CustomerSalesPlugin(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [KernelFunction]
    [Description("Retrieves the sales orders for a specific customer.")]
    public async Task<IReadOnlyList<SalesOrderEntity>> GetSalesForCustomer([Description("The customer id")] string customerId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();

        var salesOrders = await dbContext.SalesOrders
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.OrderDate)
            .ToListAsync();

        return salesOrders.AsReadOnly();
    }

    [KernelFunction]
    [Description("Retrieves all sales orders from all customers.")]
    public async Task<IReadOnlyList<SalesOrderEntity>> GetAllSales()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();

        var salesOrders = await dbContext.SalesOrders
            .OrderByDescending(s => s.OrderDate)
            .ToListAsync();

        return salesOrders.AsReadOnly();
    }

    [KernelFunction]
    [Description("Retrieves customer information by customer ID.")]
    public async Task<CustomerEntity?> GetCustomer([Description("The customer id")] string customerId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();

        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        return customer;
    }

    [KernelFunction]
    [Description("Retrieves all customers.")]
    public async Task<IReadOnlyList<CustomerEntity>> GetAllCustomers()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AthenaDbContext>();

        var customers = await dbContext.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();

        return customers.AsReadOnly();
    }
}
using Athena.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Athena.Api.Extensions;

public static class DatabaseSeederExtensions
{
    public static async Task SeedDataAsync(this AthenaDbContext context)
    {
        // Check if data already exists
        if (await context.Customers.AnyAsync() || await context.SalesOrders.AnyAsync())
        {
            return; // Data already seeded
        }

        // Seed customers
        var customers = new[]
        {
            new CustomerEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-001",
                Name = "Acme Corporation",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new CustomerEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-002",
                Name = "Global Tech Solutions",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new CustomerEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-003",
                Name = "Innovative Industries",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Seed sales orders
        var salesOrders = new[]
        {
            new SalesOrderEntity
            {
                Id = Guid.NewGuid(),
                SalesOrderId = "SO-2024-001",
                CustomerId = "CUST-001",
                OrderDate = DateTime.UtcNow.AddDays(-15),
                TotalAmount = 1250.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new SalesOrderEntity
            {
                Id = Guid.NewGuid(),
                SalesOrderId = "SO-2024-002",
                CustomerId = "CUST-002",
                OrderDate = DateTime.UtcNow.AddDays(-12),
                TotalAmount = 2300.50m,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new SalesOrderEntity
            {
                Id = Guid.NewGuid(),
                SalesOrderId = "SO-2024-003",
                CustomerId = "CUST-001",
                OrderDate = DateTime.UtcNow.AddDays(-8),
                TotalAmount = 875.25m,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new SalesOrderEntity
            {
                Id = Guid.NewGuid(),
                SalesOrderId = "SO-2024-004",
                CustomerId = "CUST-003",
                OrderDate = DateTime.UtcNow.AddDays(-5),
                TotalAmount = 3100.75m,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new SalesOrderEntity
            {
                Id = Guid.NewGuid(),
                SalesOrderId = "SO-2024-005",
                CustomerId = "CUST-002",
                OrderDate = DateTime.UtcNow.AddDays(-2),
                TotalAmount = 1680.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.SalesOrders.AddRange(salesOrders);
        await context.SaveChangesAsync();
    }
}
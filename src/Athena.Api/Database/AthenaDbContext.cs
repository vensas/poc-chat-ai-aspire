using Microsoft.EntityFrameworkCore;

namespace Athena.Api.Database;

public class AthenaDbContext : DbContext
{
    public AthenaDbContext(DbContextOptions<AthenaDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<SalesOrderEntity> SalesOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Create index on CustomerId for better query performance
            entity.HasIndex(e => e.CustomerId)
                .IsUnique()
                .HasDatabaseName("IX_Customers_CustomerId");
        });

        // Configure SalesOrder entity
        modelBuilder.Entity<SalesOrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SalesOrderId)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.CustomerId)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Create indexes for better query performance
            entity.HasIndex(e => e.SalesOrderId)
                .IsUnique()
                .HasDatabaseName("IX_SalesOrders_SalesOrderId");
            entity.HasIndex(e => e.CustomerId)
                .HasDatabaseName("IX_SalesOrders_CustomerId");
            entity.HasIndex(e => e.OrderDate)
                .HasDatabaseName("IX_SalesOrders_OrderDate");
        });

        // Configure table names to use the same schema as Marten
        modelBuilder.Entity<CustomerEntity>().ToTable("customers", "athena");
        modelBuilder.Entity<SalesOrderEntity>().ToTable("sales_orders", "athena");
    }
}
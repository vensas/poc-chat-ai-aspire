using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Athena.Api.Database;

public class AthenaDbContextFactory : IDesignTimeDbContextFactory<AthenaDbContext>
{
    public AthenaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AthenaDbContext>();
        
        // For design-time, read directly from appsettings.Development.json
        // The main application configuration is handled in Program.cs
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__postgres-default")
            ?? throw new InvalidOperationException(
                "Design-time connection string not found. " +
                "Please set the ConnectionStrings__postgres-default environment variable " +
                "or ensure the application is configured properly in Program.cs");
        
        optionsBuilder.UseNpgsql(connectionString);

        return new AthenaDbContext(optionsBuilder.Options);
    }
}
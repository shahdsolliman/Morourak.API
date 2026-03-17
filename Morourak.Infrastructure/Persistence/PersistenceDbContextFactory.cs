using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Morourak.Infrastructure.Persistence;

public class PersistenceDbContextFactory : IDesignTimeDbContextFactory<PersistenceDbContext>
{
    public PersistenceDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("PersistenceConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Could not find 'PersistenceConnection'. " +
                "For design-time operations in Development, set it in 'appsettings.Development.json'.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<PersistenceDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PersistenceDbContext(optionsBuilder.Options);
    }
}


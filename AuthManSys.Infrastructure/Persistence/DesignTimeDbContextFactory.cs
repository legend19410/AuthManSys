using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AuthManSys.Infrastructure.Persistence;

namespace AuthManSys.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthManSysDbContext>
{
    public AuthManSysDbContext CreateDbContext(string[] args)
    {
        // Build configuration from the API project's appsettings
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AuthManSys.Api");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthManSysDbContext>();
        
        // Get database provider setting
        var databaseProvider = configuration["DatabaseProvider"] ?? "SQLite";
        
        if (databaseProvider.ToUpper() == "SQLSERVER")
        {
            var sqlServerConnectionString = configuration.GetConnectionString("SqlServerConnection");
            optionsBuilder.UseSqlServer(sqlServerConnectionString);
        }
        else
        {
            var sqliteConnectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(sqliteConnectionString);
        }

        return new AuthManSysDbContext(optionsBuilder.Options);
    }
}
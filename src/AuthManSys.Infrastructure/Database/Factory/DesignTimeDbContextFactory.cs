using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using AuthManSys.Infrastructure.Database.DbContext;

namespace AuthManSys.Infrastructure.Database.Factory;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthManSysDbContext>
{
    public AuthManSysDbContext CreateDbContext(string[] args)
    {
        // Try to find configuration from current directory first (for Console app), then API project
        var configuration = new ConfigurationBuilder();

        // Check if we're running from Console app directory
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
        {
            configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
        }
        else
        {
            // Fallback to API project's appsettings
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AuthManSys.Api");
            configuration
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true);
        }

        var config = configuration.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthManSysDbContext>();
        
        // Get database provider setting
        var databaseProvider = config["DatabaseProvider"] ?? "MySQL";

        if (databaseProvider.ToUpper() == "SQLSERVER")
        {
            var sqlServerConnectionString = config.GetConnectionString("SqlServerConnection");
            optionsBuilder.UseSqlServer(sqlServerConnectionString);
        }
        else if (databaseProvider.ToUpper() == "MYSQL")
        {
            var mySqlConnectionString = config.GetConnectionString("MySqlConnection");
            optionsBuilder.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}. Supported providers are: MySQL, SqlServer");
        }

        return new AuthManSysDbContext(optionsBuilder.Options);
    }
}
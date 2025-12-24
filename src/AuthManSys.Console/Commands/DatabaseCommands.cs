using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Infrastructure.Database.DbContext;
using AuthManSys.Domain.Entities;
using AuthManSys.Infrastructure.Services;
using AuthManSys.Infrastructure.Database.Seeder;

namespace AuthManSys.Console.Commands;

public class DatabaseCommands : IDatabaseCommands
{
    private readonly AuthManSysDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DatabaseCommands(
        AuthManSysDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedDatabaseAsync()
    {
        System.Console.WriteLine("🌱 Seeding Database...");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            await MasterSeeder.SeedAllAsync(_context, _userManager, _roleManager);
            System.Console.WriteLine("✅ Database seeded successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error seeding database: {ex.Message}");
        }
    }

    public async Task RunMigrationsAsync()
    {
        System.Console.WriteLine("🔄 Running Database Migrations...");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

            if (!pendingMigrations.Any())
            {
                System.Console.WriteLine("✅ No pending migrations found. Database is up to date.");
                return;
            }

            System.Console.WriteLine($"Found {pendingMigrations.Count()} pending migrations:");
            foreach (var migration in pendingMigrations)
            {
                System.Console.WriteLine($"  - {migration}");
            }

            System.Console.Write("Apply these migrations? (y/N): ");
            var confirmation = System.Console.ReadLine();

            if (confirmation?.ToLower() != "y")
            {
                System.Console.WriteLine("❌ Migration cancelled.");
                return;
            }

            await _context.Database.MigrateAsync();
            System.Console.WriteLine("✅ Migrations applied successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error running migrations: {ex.Message}");
        }
    }

    public async Task ResetDatabaseAsync()
    {
        System.Console.WriteLine("🗑️ Reset Database");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");
        System.Console.WriteLine("⚠️ WARNING: This will delete ALL data in the database!");
        System.Console.Write("Are you sure you want to continue? (y/N): ");

        var confirmation = System.Console.ReadLine();

        if (confirmation?.ToLower() != "y")
        {
            System.Console.WriteLine("❌ Database reset cancelled.");
            return;
        }

        try
        {
            await _context.Database.EnsureDeletedAsync();
            System.Console.WriteLine("✅ Database deleted successfully!");

            await _context.Database.EnsureCreatedAsync();
            System.Console.WriteLine("✅ Database recreated successfully!");

            await MasterSeeder.SeedAllAsync(_context, _userManager, _roleManager);
            System.Console.WriteLine("✅ Database seeded with initial data!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error resetting database: {ex.Message}");
        }
    }

    public async Task CheckDatabaseStatusAsync()
    {
        System.Console.WriteLine("📊 Database Status");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            System.Console.WriteLine($"Connection Status: {(canConnect ? "✅ Connected" : "❌ Cannot connect")}");

            if (!canConnect)
                return;

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

            System.Console.WriteLine($"Applied Migrations: {appliedMigrations.Count()}");
            System.Console.WriteLine($"Pending Migrations: {pendingMigrations.Count()}");

            if (pendingMigrations.Any())
            {
                System.Console.WriteLine("⚠️ Pending migrations found:");
                foreach (var migration in pendingMigrations)
                {
                    System.Console.WriteLine($"  - {migration}");
                }
            }

            var userCount = await _context.Users.CountAsync();
            var roleCount = await _context.Roles.CountAsync();

            System.Console.WriteLine($"Users: {userCount}");
            System.Console.WriteLine($"Roles: {roleCount}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error checking database status: {ex.Message}");
        }
    }
}
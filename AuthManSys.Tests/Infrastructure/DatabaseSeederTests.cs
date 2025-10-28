using Microsoft.EntityFrameworkCore;
using AuthManSys.Infrastructure.Persistence;
using AuthManSys.Domain.Entities;

namespace AuthManSys.Tests.Infrastructure;

public class DatabaseSeederTests
{
    private AuthManSysDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AuthManSysDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AuthManSysDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyDatabase_SeedsUsersAndRoles()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        await DatabaseSeeder.SeedAsync(context);

        // Assert
        var users = await context.Users.ToListAsync();
        var roles = await context.Roles.ToListAsync();

        Assert.True(users.Count >= 6, "Should seed at least 6 users");
        Assert.True(roles.Count >= 3, "Should have at least 3 roles");
        
        // Check specific users exist
        Assert.Contains(users, u => u.Username == "admin");
        Assert.Contains(users, u => u.Username == "jdoe");
        Assert.Contains(users, u => u.Username == "jsmith");
        
        // Check specific roles exist
        Assert.Contains(roles, r => r.Name == "Administrator");
        Assert.Contains(roles, r => r.Name == "User");
        Assert.Contains(roles, r => r.Name == "Manager");
    }

    [Fact]
    public async Task SeedAsync_WithExistingUsers_DoesNotDuplicateData()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Add a test user first
        var existingUser = new User
        {
            Username = "existing",
            Email = "existing@test.com",
            PasswordHash = "hash",
            FirstName = "Existing",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();
        
        var initialUserCount = await context.Users.CountAsync();

        // Act
        await DatabaseSeeder.SeedAsync(context);

        // Assert
        var finalUserCount = await context.Users.CountAsync();
        Assert.Equal(initialUserCount, finalUserCount); // Should not add more users
    }

    [Fact]
    public async Task SeedAsync_CreatesUsersWithCorrectProperties()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        await DatabaseSeeder.SeedAsync(context);

        // Assert
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        Assert.NotNull(adminUser);
        Assert.Equal("admin@authmansys.com", adminUser.Email);
        Assert.Equal("System", adminUser.FirstName);
        Assert.Equal("Administrator", adminUser.LastName);
        Assert.True(adminUser.IsActive);
        Assert.NotEmpty(adminUser.PasswordHash);

        var inactiveUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "mjohnson");
        Assert.NotNull(inactiveUser);
        Assert.False(inactiveUser.IsActive); // This user should be inactive
    }

    [Fact]
    public async Task SeedAsync_CreatesRolesWithCorrectProperties()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        await DatabaseSeeder.SeedAsync(context);

        // Assert
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
        Assert.NotNull(adminRole);
        Assert.Equal("Full system access with all permissions", adminRole.Description);
        Assert.True(adminRole.IsActive);

        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        Assert.NotNull(userRole);
        Assert.Equal("Standard user access with basic permissions", userRole.Description);
        Assert.True(userRole.IsActive);
    }

    [Fact]
    public async Task SeedAsync_WithPreExistingRoles_AddsUsersOnly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Add roles first (simulating migration seed data)
        var existingRoles = new[]
        {
            new Role { Name = "Administrator", Description = "Full system access", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Name = "User", Description = "Standard user access", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Name = "Manager", Description = "Management level access", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        
        context.Roles.AddRange(existingRoles);
        await context.SaveChangesAsync();
        
        var initialRoleCount = await context.Roles.CountAsync();

        // Act
        await DatabaseSeeder.SeedAsync(context);

        // Assert
        var finalRoleCount = await context.Roles.CountAsync();
        var userCount = await context.Users.CountAsync();
        
        Assert.Equal(initialRoleCount, finalRoleCount); // Should not add more roles
        Assert.True(userCount >= 6, "Should add users even when roles exist");
    }

    [Fact]
    public async Task SeedAsync_EnsuresDatabaseExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        // Note: In-memory database always exists, but this tests the method call

        // Act & Assert - Should not throw
        await DatabaseSeeder.SeedAsync(context);
        
        // Verify database operations work
        var userCount = await context.Users.CountAsync();
        Assert.True(userCount > 0);
    }
}
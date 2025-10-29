using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.Persistence;
using AuthManSys.Domain.Entities;

namespace AuthManSys.Tests.Infrastructure;

public class IdentitySeederTests
{
    private (AuthManSysDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) CreateTestServices()
    {
        var services = new ServiceCollection();

        // Add DbContext with in-memory database
        services.AddDbContext<AuthManSysDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add Identity services
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthManSysDbContext>()
            .AddDefaultTokenProviders();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        var serviceProvider = services.BuildServiceProvider();

        var context = serviceProvider.GetRequiredService<AuthManSysDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        context.Database.EnsureCreated();

        return (context, userManager, roleManager);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyDatabase_SeedsUsersAndRoles()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var users = await userManager.Users.ToListAsync();
        var roles = await roleManager.Roles.ToListAsync();

        Assert.True(users.Count >= 6, "Should seed at least 6 users");
        Assert.True(roles.Count >= 4, "Should have at least 4 roles");

        // Check specific users exist
        Assert.Contains(users, u => u.UserName == "admin");
        Assert.Contains(users, u => u.UserName == "jdoe");
        Assert.Contains(users, u => u.UserName == "jsmith");

        // Check specific roles exist
        Assert.Contains(roles, r => r.Name == "Administrator");
        Assert.Contains(roles, r => r.Name == "User");
        Assert.Contains(roles, r => r.Name == "Manager");
        Assert.Contains(roles, r => r.Name == "ReadOnly");
    }

    [Fact]
    public async Task SeedAsync_WithExistingUsers_DoesNotDuplicateData()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Add a test user first
        var existingUser = new ApplicationUser
        {
            UserName = "existing",
            Email = "existing@test.com",
            EmailConfirmed = true,
            FirstName = "Existing",
            LastName = "User",
            EmailConfirmationToken = string.Empty,
            PasswordResetToken = string.Empty,
            RequestVerificationToken = string.Empty,
            TermsConditionsAccepted = true,
            LastPasswordChangedDate = DateTime.UtcNow
        };

        await userManager.CreateAsync(existingUser, "Password123!");
        var initialUserCount = await userManager.Users.CountAsync();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var finalUserCount = await userManager.Users.CountAsync();
        Assert.Equal(initialUserCount, finalUserCount); // Should not add more users when users already exist
    }

    [Fact]
    public async Task SeedAsync_CreatesUsersWithCorrectProperties()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var adminUser = await userManager.FindByNameAsync("admin");
        Assert.NotNull(adminUser);
        Assert.Equal("admin@authmansys.com", adminUser.Email);
        Assert.Equal("System", adminUser.FirstName);
        Assert.Equal("Administrator", adminUser.LastName);
        Assert.True(adminUser.EmailConfirmed);
        Assert.True(adminUser.TermsConditionsAccepted);

        var jdoeUser = await userManager.FindByNameAsync("jdoe");
        Assert.NotNull(jdoeUser);
        Assert.Equal("john.doe@authmansys.com", jdoeUser.Email);
        Assert.Equal("John", jdoeUser.FirstName);
        Assert.Equal("Doe", jdoeUser.LastName);
    }

    [Fact]
    public async Task SeedAsync_AssignsCorrectRolesToUsers()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var adminUser = await userManager.FindByNameAsync("admin");
        var adminRoles = await userManager.GetRolesAsync(adminUser!);
        Assert.Contains("Administrator", adminRoles);

        var managerUser = await userManager.FindByNameAsync("jsmith");
        var managerRoles = await userManager.GetRolesAsync(managerUser!);
        Assert.Contains("Manager", managerRoles);

        var regularUser = await userManager.FindByNameAsync("jdoe");
        var userRoles = await userManager.GetRolesAsync(regularUser!);
        Assert.Contains("User", userRoles);

        var readOnlyUser = await userManager.FindByNameAsync("adavis");
        var readOnlyRoles = await userManager.GetRolesAsync(readOnlyUser!);
        Assert.Contains("ReadOnly", readOnlyRoles);
    }

    [Fact]
    public async Task SeedAsync_CreatesAllRequiredRoles()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var expectedRoles = new[] { "Administrator", "Manager", "User", "ReadOnly" };

        foreach (var roleName in expectedRoles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            Assert.True(roleExists, $"Role '{roleName}' should exist");
        }
    }

    [Fact]
    public async Task SeedAsync_WithPreExistingRoles_DoesNotDuplicateRoles()
    {
        // Arrange
        var (context, userManager, roleManager) = CreateTestServices();

        // Add roles first
        await roleManager.CreateAsync(new IdentityRole("Administrator"));
        await roleManager.CreateAsync(new IdentityRole("User"));

        var initialRoleCount = await roleManager.Roles.CountAsync();

        // Act
        await IdentitySeeder.SeedAsync(context, userManager, roleManager);

        // Assert
        var finalRoleCount = await roleManager.Roles.CountAsync();
        Assert.True(finalRoleCount >= initialRoleCount); // Should add missing roles but not duplicate existing ones

        // Verify all expected roles exist
        var expectedRoles = new[] { "Administrator", "Manager", "User", "ReadOnly" };
        foreach (var roleName in expectedRoles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            Assert.True(roleExists, $"Role '{roleName}' should exist");
        }
    }
}
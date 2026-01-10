using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AuthManSys.Api.Models;
using AuthManSys.Api.Models.Responses;
using AuthManSys.Infrastructure.Database.EFCore.DbContext;

namespace AuthManSys.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuthManSysDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<AuthManSysDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Ensure database is created and seeded
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AuthManSysDbContext>();
                context.Database.EnsureCreated();
                // Note: Seeder will run automatically via Program.cs
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Post_Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal("admin", authResponse.Username);
        Assert.Equal("admin@authmansys.com", authResponse.Email);
    }

    [Fact]
    public async Task Post_Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_TestEndpoint_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_TestEndpoint_WithValidToken_ReturnsSuccess()
    {
        // Arrange - First login to get a token
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Add the token to subsequent requests
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await _client.GetAsync("/api/test");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello", content); // Assuming test endpoint returns some greeting
    }

    [Fact]
    public async Task Post_ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange - Get a valid token first
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act
        var response = await _client.PostAsync($"/api/auth/validate?token={authResponse!.Token}", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("true", content.ToLower());
    }

    [Fact]
    public async Task Post_ValidateToken_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        const string invalidToken = "invalid.jwt.token";

        // Act
        var response = await _client.PostAsync($"/api/auth/validate?token={invalidToken}", null);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UserInfo_WithValidToken_ReturnsUserData()
    {
        // Arrange - Login first
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/userinfo");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("admin", content);
        Assert.Contains("admin@authmansys.com", content);
    }
}
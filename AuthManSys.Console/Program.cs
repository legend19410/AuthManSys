using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthManSys.Api.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
    })
    .Build();

var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient();

string baseUrl = "http://localhost:5000";
string? currentToken = null;

Console.WriteLine("=== AuthManSys Console Testing Tool ===");
Console.WriteLine("Available commands:");
Console.WriteLine("  login <username> <password> - Login with credentials");
Console.WriteLine("  validate - Validate current token");
Console.WriteLine("  userinfo - Get user information (requires auth)");
Console.WriteLine("  test - Test protected endpoint (requires auth)");
Console.WriteLine("  weather - Test weather endpoint (no auth required)");
Console.WriteLine("  set-url <url> - Set base URL (default: http://localhost:5000)");
Console.WriteLine("  token - Show current token");
Console.WriteLine("  clear - Clear current token");
Console.WriteLine("  help - Show this help");
Console.WriteLine("  exit - Exit application");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrEmpty(input))
        continue;
        
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();
    
    try
    {
        switch (command)
        {
            case "login":
                await HandleLogin(httpClient, parts);
                break;
                
            case "validate":
                await HandleValidate(httpClient);
                break;
                
            case "userinfo":
                await HandleUserInfo(httpClient);
                break;
                
            case "test":
                await HandleTest(httpClient);
                break;
                
            case "weather":
                await HandleWeather(httpClient);
                break;
                
            case "set-url":
                HandleSetUrl(parts);
                break;
                
            case "token":
                HandleToken();
                break;
                
            case "clear":
                HandleClear();
                break;
                
            case "help":
                ShowHelp();
                break;
                
            case "exit":
                Console.WriteLine("Goodbye!");
                return;
                
            default:
                Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

async Task HandleLogin(HttpClient client, string[] parts)
{
    if (parts.Length < 3)
    {
        Console.WriteLine("Usage: login <username> <password>");
        return;
    }
    
    var loginRequest = new LoginRequest
    {
        Username = parts[1],
        Password = parts[2]
    };
    
    var json = JsonSerializer.Serialize(loginRequest);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    Console.WriteLine($"Attempting login for user: {loginRequest.Username}...");
    
    var response = await client.PostAsync($"{baseUrl}/api/auth/login", content);
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        currentToken = authResponse?.Token;
        
        Console.WriteLine("✓ Login successful!");
        Console.WriteLine($"Username: {authResponse?.Username}");
        Console.WriteLine($"Email: {authResponse?.Email}");
        Console.WriteLine($"Role: {authResponse?.Role}");
        Console.WriteLine($"Expires: {authResponse?.ExpiresAt}");
        Console.WriteLine("Token saved for subsequent requests.");
        
        // Set authorization header for future requests
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentToken);
    }
    else
    {
        Console.WriteLine($"✗ Login failed: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}

async Task HandleValidate(HttpClient client)
{
    if (string.IsNullOrEmpty(currentToken))
    {
        Console.WriteLine("No token available. Please login first.");
        return;
    }
    
    Console.WriteLine("Validating current token...");
    
    var response = await client.PostAsync($"{baseUrl}/api/auth/validate", null);
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("✓ Token is valid!");
        Console.WriteLine($"Response: {responseContent}");
    }
    else
    {
        Console.WriteLine($"✗ Token validation failed: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}

async Task HandleUserInfo(HttpClient client)
{
    if (string.IsNullOrEmpty(currentToken))
    {
        Console.WriteLine("No token available. Please login first.");
        return;
    }
    
    Console.WriteLine("Fetching user information...");
    
    var response = await client.GetAsync($"{baseUrl}/api/auth/user-info");
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("✓ User info retrieved successfully!");
        Console.WriteLine($"Response: {responseContent}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to get user info: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}

async Task HandleTest(HttpClient client)
{
    Console.WriteLine("Testing protected endpoint...");
    
    var response = await client.GetAsync($"{baseUrl}/api/test");
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("✓ Test endpoint responded successfully!");
        Console.WriteLine($"Response: {responseContent}");
    }
    else
    {
        Console.WriteLine($"✗ Test endpoint failed: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}

async Task HandleWeather(HttpClient client)
{
    Console.WriteLine("Testing weather endpoint (no auth required)...");
    
    var response = await client.GetAsync($"{baseUrl}/weatherforecast");
    var responseContent = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("✓ Weather endpoint responded successfully!");
        Console.WriteLine($"Response: {responseContent}");
    }
    else
    {
        Console.WriteLine($"✗ Weather endpoint failed: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}

void HandleSetUrl(string[] parts)
{
    if (parts.Length < 2)
    {
        Console.WriteLine("Usage: set-url <url>");
        Console.WriteLine($"Current URL: {baseUrl}");
        return;
    }
    
    baseUrl = parts[1].TrimEnd('/');
    Console.WriteLine($"Base URL set to: {baseUrl}");
}

void HandleToken()
{
    if (string.IsNullOrEmpty(currentToken))
    {
        Console.WriteLine("No token available.");
    }
    else
    {
        Console.WriteLine($"Current token: {currentToken}");
    }
}

void HandleClear()
{
    currentToken = null;
    httpClient.DefaultRequestHeaders.Authorization = null;
    Console.WriteLine("Token cleared.");
}

void ShowHelp()
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("  login <username> <password> - Login with credentials");
    Console.WriteLine("    Example: login admin Admin123!");
    Console.WriteLine("  validate - Validate current token");
    Console.WriteLine("  userinfo - Get user information (requires auth)");
    Console.WriteLine("  test - Test protected endpoint (requires auth)");
    Console.WriteLine("  weather - Test weather endpoint (no auth required)");
    Console.WriteLine("  set-url <url> - Set base URL (default: http://localhost:5000)");
    Console.WriteLine("    Example: set-url https://localhost:7000");
    Console.WriteLine("  token - Show current token");
    Console.WriteLine("  clear - Clear current token");
    Console.WriteLine("  help - Show this help");
    Console.WriteLine("  exit - Exit application");
    Console.WriteLine();
    Console.WriteLine("Quick test sequence:");
    Console.WriteLine("1. login admin Admin123!");
    Console.WriteLine("2. validate");
    Console.WriteLine("3. userinfo");
    Console.WriteLine("4. test");
}

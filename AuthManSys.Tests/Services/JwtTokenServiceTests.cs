using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using AuthManSys.Api.Models;
using AuthManSys.Api.Services;

namespace AuthManSys.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly IJwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsATestSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };

        var options = Options.Create(_jwtSettings);
        _jwtTokenService = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateToken_WithValidParameters_ReturnsValidJwt()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "User";

        // Act
        var token = _jwtTokenService.GenerateToken(username, email, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify it's a valid JWT format
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_ValidToken_ContainsCorrectClaims()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "Administrator";

        // Act
        var token = _jwtTokenService.GenerateToken(username, email, role);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);

        Assert.Equal(username, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal(_jwtSettings.Issuer, jwt.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ValidToken_HasCorrectExpiration()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "User";
        var beforeTokenGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtTokenService.GenerateToken(username, email, role);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        var expectedExpiry = beforeTokenGeneration.AddMinutes(_jwtSettings.ExpiryMinutes);
        var actualExpiry = jwt.ValidTo;
        
        // Allow for a small time difference due to execution time
        var timeDifference = Math.Abs((expectedExpiry - actualExpiry).TotalSeconds);
        Assert.True(timeDifference < 5, $"Token expiry time difference is too large: {timeDifference} seconds");
    }

    [Theory]
    [InlineData("", "test@example.com", "User")]
    [InlineData("testuser", "", "User")]
    [InlineData("testuser", "test@example.com", "")]
    public void GenerateToken_WithEmptyParameters_ThrowsArgumentException(string username, string email, string role)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _jwtTokenService.GenerateToken(username, email, role));
    }

    [Theory]
    [InlineData(null, "test@example.com", "User")]
    [InlineData("testuser", null, "User")]
    [InlineData("testuser", "test@example.com", null)]
    public void CreateAuthResponse_WithNullParameters_ThrowsArgumentNullException(string? username, string? email, string? role)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jwtTokenService.CreateAuthResponse(username!, email!, role!));
    }

    [Fact]
    public void CreateAuthResponse_WithValidParameters_ReturnsAuthResponse()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "User";

        // Act
        var authResponse = _jwtTokenService.CreateAuthResponse(username, email, role);

        // Assert
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal(username, authResponse.Username);
        Assert.Equal(email, authResponse.Email);
        Assert.Equal(role, authResponse.Role);
        Assert.True(authResponse.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void CreateAuthResponse_TokenIsValidJwt()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "Administrator";

        // Act
        var authResponse = _jwtTokenService.CreateAuthResponse(username, email, role);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(authResponse.Token));
        
        var jwt = tokenHandler.ReadJwtToken(authResponse.Token);
        Assert.Equal(username, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Theory]
    [InlineData("", "test@example.com", "User")]
    [InlineData("testuser", "", "User")]
    [InlineData("testuser", "test@example.com", "")]
    public void CreateAuthResponse_WithEmptyParameters_ThrowsArgumentException(string username, string email, string role)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _jwtTokenService.CreateAuthResponse(username, email, role));
    }
}
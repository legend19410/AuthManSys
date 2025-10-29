using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Security.Services;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Tests.Services;

public class SecurityServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly ISecurityService _securityService;

    public SecurityServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsATestSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        };

        var options = Options.Create(_jwtSettings);
        _securityService = new SecurityService(options);
    }

    [Fact]
    public void GenerateToken_WithValidParameters_ReturnsValidJwt()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";

        // Act
        var token = _securityService.GenerateToken(username, email);

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

        // Act
        var token = _securityService.GenerateToken(username, email);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);

        Assert.Equal(username, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(username, jwt.Claims.First(c => c.Type == "username").Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == "email").Value);
        Assert.Equal(_jwtSettings.Issuer, jwt.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ValidToken_HasCorrectExpiration()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        var beforeTokenGeneration = DateTime.UtcNow;

        // Act
        var token = _securityService.GenerateToken(username, email);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);

        var expectedExpiry = beforeTokenGeneration.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var actualExpiry = jwt.ValidTo;

        // Allow for a small time difference due to execution time
        var timeDifference = Math.Abs((expectedExpiry - actualExpiry).TotalSeconds);
        Assert.True(timeDifference < 5, $"Token expiry time difference is too large: {timeDifference} seconds");
    }

    [Theory]
    [InlineData("", "test@example.com")]
    [InlineData("testuser", "")]
    public void GenerateToken_WithEmptyParameters_ThrowsArgumentException(string username, string email)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _securityService.GenerateToken(username, email));
    }

    [Theory]
    [InlineData(null, "test@example.com")]
    [InlineData("testuser", null)]
    public void GenerateToken_WithNullParameters_ThrowsArgumentNullException(string? username, string? email)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _securityService.GenerateToken(username!, email!));
    }
}
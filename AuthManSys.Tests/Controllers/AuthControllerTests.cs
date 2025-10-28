using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using AuthManSys.Api.Controllers;
using AuthManSys.Api.Models;
using AuthManSys.Api.Services;

namespace AuthManSys.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IJwtTokenService> _mockJwtService;
    private readonly AuthController _controller;
    private readonly DefaultCredentials _defaultCredentials;

    public AuthControllerTests()
    {
        _mockJwtService = new Mock<IJwtTokenService>();

        _defaultCredentials = new DefaultCredentials
        {
            Username = "admin",
            Password = "Admin123!",
            Email = "admin@example.com",
            Role = "Administrator"
        };

        var credentialsOptions = Options.Create(_defaultCredentials);
        _controller = new AuthController(_mockJwtService.Object, credentialsOptions);

    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = _defaultCredentials.Username,
            Password = _defaultCredentials.Password
        };

        var expectedAuthResponse = new AuthResponse
        {
            Token = "mock.jwt.token",
            Username = _defaultCredentials.Username,
            Email = _defaultCredentials.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockJwtService.Setup(x => x.CreateAuthResponse(_defaultCredentials.Username, _defaultCredentials.Email, _defaultCredentials.Role))
                      .Returns(expectedAuthResponse);

        // Act
        var result = _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AuthResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        
        Assert.Equal(expectedAuthResponse.Token, response.Token);
        Assert.Equal(_defaultCredentials.Username, response.Username);
        Assert.Equal(_defaultCredentials.Email, response.Email);
    }

    [Fact]
    public void Login_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            Password = "Password123!"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AuthResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Username and password are required", badRequestResult.Value);
    }

    [Fact]
    public void Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "Password123!"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AuthResponse>>(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }

    [Fact]
    public void Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = _defaultCredentials.Username,
            Password = "WrongPassword"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AuthResponse>>(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }

    [Fact]
    public void Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = _defaultCredentials.Username,
            Password = ""
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AuthResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Username and password are required", badRequestResult.Value);
    }


}
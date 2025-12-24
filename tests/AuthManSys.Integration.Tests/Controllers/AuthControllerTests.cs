using Microsoft.AspNetCore.Mvc;
using Moq;
using MediatR;
using AuthManSys.Api.Controllers;
using AuthManSys.Api.Models;
using AuthManSys.Application.Modules.Auth.Login.Commands;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Domain.Entities;

namespace AuthManSys.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IIdentityProvider> _mockIdentityProvider;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockIdentityProvider = new Mock<IIdentityProvider>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null).Object,
            new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
            new Mock<Microsoft.AspNetCore.Identity.IUserConfirmation<ApplicationUser>>().Object,
            null, null, null, null);
        _controller = new AuthController(_mockMediator.Object, _mockLogger.Object, _mockIdentityProvider.Object, _mockUserRepository.Object, _mockSignInManager.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var expectedLoginResponse = new LoginResponse
        {
            Token = "mock.jwt.token",
            Username = "admin",
            Email = "admin@example.com",
            Roles = new List<string> { "Administrator" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), default))
                    .ReturnsAsync(expectedLoginResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);

        Assert.Equal(expectedLoginResponse.Token, response.Token);
        Assert.Equal(expectedLoginResponse.Username, response.Username);
        Assert.Equal(expectedLoginResponse.Email, response.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "invalid",
            Password = "invalid"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), default))
                    .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithException_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "test",
            Password = "test"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), default))
                    .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Contains("Login failed", badRequestResult.Value?.ToString());
    }


}
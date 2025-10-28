using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Security.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ISecurityService _securityService;


    public LoginCommandHandler(
        IIdentityExtension identityExtension,
        ISecurityService securityService
    )
    {
        _identityExtension = identityExtension;
        _securityService = securityService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _identityExtension.FindByUserNameAsync(request.Username);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid username or password");
        }

        // Verify password
        var isPasswordValid = await _identityExtension.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid username or password");
        }

        // Check if email is confirmed
        var isEmailConfirmed = await _identityExtension.IsEmailConfirmedAsync(request.Username);
        if (!isEmailConfirmed)
        {
            throw new UnauthorizedException("Email address is not confirmed");
        }

        // Get user roles
        var roles = await _identityExtension.GetUserRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        //generate token
        var token = _securityService.GenerateToken(user.UserName, user.Email);

        // Create authentication response
        var authResponse = new LoginResponse
        {
            Username = user.UserName ?? request.Username,
            Email = user.Email ?? "",
            Roles = roles.ToList(),
            Token = token,
        };

        return authResponse;
    }
}

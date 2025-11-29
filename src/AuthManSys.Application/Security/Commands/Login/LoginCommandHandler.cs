using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.TwoFactor.Commands;
using System.IdentityModel.Tokens.Jwt;

namespace AuthManSys.Application.Security.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly IActivityLogService _activityLogService;
    private readonly IMediator _mediator;

    public LoginCommandHandler(
        IIdentityExtension identityExtension,
        IActivityLogService activityLogService,
        IMediator mediator
    )
    {
        _identityExtension = identityExtension;
        _activityLogService = activityLogService;
        _mediator = mediator;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the user
            var user = await _identityExtension.FindByUserNameAsync(request.Username);
            if (user == null)
            {
                // Log failed login attempt
                await _activityLogService.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.LoginFailed,
                    description: $"Login attempt failed for non-existent user: {request.Username}",
                    metadata: new { Username = request.Username, Reason = "UserNotFound" },
                    cancellationToken: cancellationToken);

                throw new UnauthorizedException("Invalid username or password");
            }

            // Verify password
            var isPasswordValid = await _identityExtension.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                // Log failed login attempt
                await _activityLogService.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.LoginFailed,
                    description: $"Login attempt failed for user: {user.UserName} - Invalid password",
                    metadata: new { Username = request.Username, Reason = "InvalidPassword" },
                    cancellationToken: cancellationToken);

                throw new UnauthorizedException("Invalid username or password");
            }

        // Check if email is confirmed
        // var isEmailConfirmed = await _identityExtension.IsEmailConfirmedAsync(request.Username);
        // if (!isEmailConfirmed)
        // {
        //     throw new UnauthorizedException("Email address is not confirmed");
        // }

        // Check if two-factor authentication is required
        if (user.IsTwoFactorEnabled)
        {
            // Log two-factor required
            await _activityLogService.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.TwoFactorRequired,
                description: $"Two-factor authentication required for user: {user.UserName}",
                metadata: new { Username = request.Username },
                cancellationToken: cancellationToken);

            // Automatically send 2FA code
            var sendCodeCommand = new SendTwoFactorCodeCommand(request.Username);
            var codeResult = await _mediator.Send(sendCodeCommand, cancellationToken);

            if (!codeResult.IsCodeSent)
            {
                // If we can't send the 2FA code, log and throw error
                await _activityLogService.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.TwoFactorCodeRequestFailed,
                    description: $"Failed to send 2FA code during login for user: {user.UserName}",
                    metadata: new { Username = request.Username, Reason = "FailedToSendCode", Error = codeResult.Message },
                    cancellationToken: cancellationToken);

                throw new UnauthorizedException("Failed to send verification code. Please try again.");
            }

            return new LoginResponse
            {
                RequiresTwoFactor = true,
                Username = user.UserName ?? request.Username,
                Email = user.Email ?? "",
                Message = "Two-factor authentication is required. A verification code has been sent to your email."
            };
        }

        // Update last login timestamp
        user.LastLoginAt = JamaicaTimeHelper.Now;
        await _identityExtension.UpdateUserAsync(user);

        // Get user roles
        var roles = await _identityExtension.GetUserRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        //generate token
        var token = _identityExtension.GenerateToken(user.UserName, user.Email, user.Id);

        string? refreshToken = null;
        DateTime? refreshTokenExpiration = null;

        // Generate refresh token if RememberMe is true
        if (request.RememberMe)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.ReadJwtToken(token);
            refreshToken = await _identityExtension.GenerateRefreshTokenAsync(user, jwtToken.Id);
            refreshTokenExpiration = JamaicaTimeHelper.Now.AddDays(30); // This should match your JWT settings
        }

        // Log successful login
        await _activityLogService.LogActivityAsync(
            userId: user.UserId,
            eventType: ActivityEventType.LoginSuccess,
            description: $"Successful login for user: {user.UserName}",
            metadata: new {
                Username = request.Username,
                RememberMe = request.RememberMe,
                Roles = roles.ToArray()
            },
            cancellationToken: cancellationToken);

        // Create authentication response
        var authResponse = new LoginResponse
        {
            Username = user.UserName ?? request.Username,
            Email = user.Email ?? "",
            Roles = roles.ToList(),
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
            RequiresTwoFactor = false
        };

        return authResponse;
        }
        catch (UnauthorizedException)
        {
            // Re-throw authorization exceptions without additional logging
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            await _activityLogService.LogActivityAsync(
                userId: null,
                eventType: ActivityEventType.LoginError,
                description: $"Unexpected error during login for username: {request.Username}",
                metadata: new {
                    Username = request.Username,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                },
                cancellationToken: cancellationToken);

            throw;
        }
    }
}

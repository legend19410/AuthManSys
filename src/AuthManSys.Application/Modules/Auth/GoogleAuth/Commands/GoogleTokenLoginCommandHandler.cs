using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Services;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Enums;
using AuthManSys.Domain.Entities;

namespace AuthManSys.Application.Modules.Auth.GoogleAuth.Commands;

public class GoogleTokenLoginCommandHandler : IRequestHandler<GoogleTokenLoginCommand, LoginResponse>
{
    private readonly IGoogleTokenService _googleTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<GoogleTokenLoginCommandHandler> _logger;

    public GoogleTokenLoginCommandHandler(
        IGoogleTokenService googleTokenService,
        IUserRepository userRepository,
        IJwtService jwtService,
        IActivityLogRepository activityLogService,
        ILogger<GoogleTokenLoginCommandHandler> logger)
    {
        _googleTokenService = googleTokenService;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _activityLogRepository = activityLogService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(GoogleTokenLoginCommand request, CancellationToken cancellationToken)
    {
        // Verify Google token
        var googlePayload = await _googleTokenService.VerifyTokenAsync(request.IdToken);
        if (googlePayload == null)
        {
            await _activityLogRepository.LogActivityAsync(
                userId: null,
                eventType: ActivityEventType.LoginFailed,
                description: $"Google token login failed - Invalid token for username: {request.Username ?? "auto-generated"}",
                metadata: new { Username = request.Username ?? "auto-generated", Reason = "InvalidGoogleToken" },
                cancellationToken: cancellationToken);

            throw new UnauthorizedException("Invalid Google token");
        }

        User? user = null;

        // If username is provided, check if user exists by username first
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            user = await _userRepository.FindByUserNameAsync(request.Username);
        }

        if (user == null)
        {
            // Try to find by Google ID or email from token
            // TODO: Need GetUserByGoogleIdAsync method in IUserRepository
            // user = await _userRepository.GetUserByGoogleIdAsync(googlePayload.Subject);
            if (user == null)
            {
                user = await _userRepository.FindByEmailAsync(googlePayload.Email);
            }
        }

        // If user doesn't exist, create new user with provided username or email and Google details
        if (user == null)
        {
            // Use provided username or fall back to email if username is not provided
            var username = !string.IsNullOrWhiteSpace(request.Username)
                ? request.Username
                : googlePayload.Email;

            var createUserResult = await _userRepository.CreateUserAsync(
                username,
                googlePayload.Email,
                Guid.NewGuid().ToString(), // Random password since it won't be used for Google auth
                googlePayload.GivenName ?? "",
                googlePayload.FamilyName ?? "");

            if (!createUserResult.Succeeded)
            {
                await _activityLogRepository.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.LoginError,
                    description: $"Failed to create new user during Google login for username: {username}",
                    metadata: new {
                        Username = username,
                        Email = googlePayload.Email,
                        Errors = createUserResult.Errors.ToArray()
                    },
                    cancellationToken: cancellationToken);

                throw new UnauthorizedException($"Failed to create user: {string.Join(", ", createUserResult.Errors)}");
            }

            user = await _userRepository.FindByUserNameAsync(username);
            if (user == null)
            {
                throw new UnauthorizedException("Failed to retrieve created user");
            }

            // Set Google-specific properties for new user
            user.GoogleId = googlePayload.Subject;
            user.GoogleEmail = googlePayload.Email;
            user.IsGoogleAccount = true;
            user.GoogleLinkedAt = JamaicaTimeHelper.Now;
            user.EmailConfirmed = true; // Auto-confirm since Google verified it

            await _userRepository.UpdateUserAsync(user);

            await _activityLogRepository.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.UserCreated,
                description: $"New user created via Google login: {user.UserName}",
                metadata: new {
                    Username = request.Username,
                    Email = googlePayload.Email,
                    GoogleId = googlePayload.Subject,
                    Method = "GoogleTokenLogin"
                },
                cancellationToken: cancellationToken);
        }
        else if (user.GoogleId != googlePayload.Subject)
        {
            // Link Google account to existing user
            // TODO: Need LinkGoogleAccountAsync method or separate service
            // _identityProvider.LinkGoogleAccountAsync(user, googlePayload.Subject, googlePayload.Email, googlePayload.Picture);

            // For now, update the user's Google properties directly
            user.GoogleId = googlePayload.Subject;
            user.GoogleEmail = googlePayload.Email;
            user.IsGoogleAccount = true;
            user.GoogleLinkedAt = JamaicaTimeHelper.Now;
            await _userRepository.UpdateUserAsync(user);

            await _activityLogRepository.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.AccountLinked,
                description: $"Google account linked to existing user: {user.UserName}",
                metadata: new {
                    Username = user.UserName,
                    GoogleEmail = googlePayload.Email,
                    GoogleId = googlePayload.Subject
                },
                cancellationToken: cancellationToken);
        }

        // Update last login timestamp
        user.LastLoginAt = JamaicaTimeHelper.Now;
        await _userRepository.UpdateUserAsync(user);

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user);

        // Generate JWT token
        var token = _jwtService.GenerateAccessToken(user.UserName!, user.Email!, user.Id, roles);

        // Log successful login
        await _activityLogRepository.LogActivityAsync(
            userId: user.UserId,
            eventType: ActivityEventType.LoginSuccess,
            description: $"Successful Google token login for user: {user.UserName}",
            metadata: new {
                Username = user.UserName,
                Email = user.Email,
                GoogleId = user.GoogleId,
                Roles = roles.ToArray(),
                Method = "GoogleTokenLogin"
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("User {Username} ({Email}) logged in successfully via Google token", user.UserName, user.Email);

        return new LoginResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList(),
            RequiresTwoFactor = false,
            Message = "Login successful"
        };
    }
}
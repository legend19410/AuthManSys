using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.TwoFactor.Commands;

public class SendTwoFactorCodeCommandHandler : IRequestHandler<SendTwoFactorCodeCommand, SendTwoFactorCodeResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<SendTwoFactorCodeCommandHandler> _logger;

    public SendTwoFactorCodeCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        IActivityLogService activityLogService,
        ILogger<SendTwoFactorCodeCommandHandler> logger)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<SendTwoFactorCodeResponse> Handle(SendTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found", request.Username);

                // Log failed 2FA code request for non-existent user
                await _activityLogService.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.TwoFactorCodeRequestFailed,
                    description: $"Two-factor code requested for non-existent user: {request.Username}",
                    metadata: new { Username = request.Username, Reason = "UserNotFound" },
                    cancellationToken: cancellationToken);

                return new SendTwoFactorCodeResponse
                {
                    IsCodeSent = false,
                    Message = "User not found"
                };
            }

            if (!user.IsTwoFactorEnabled)
            {
                _logger.LogWarning("Two-factor authentication is not enabled for user {Username}", request.Username);

                // Log failed 2FA code request for user with 2FA disabled
                await _activityLogService.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.TwoFactorCodeRequestFailed,
                    description: $"Two-factor code requested for user with 2FA disabled: {user.UserName}",
                    metadata: new { Username = request.Username, Reason = "TwoFactorNotEnabled" },
                    cancellationToken: cancellationToken);

                return new SendTwoFactorCodeResponse
                {
                    IsCodeSent = false,
                    Message = "Two-factor authentication is not enabled for this account"
                };
            }

            // Generate new 2FA code
            var code = _twoFactorService.GenerateTwoFactorCode();
            var expiration = _twoFactorService.GetCodeExpiration();

            // Update user with new code
            var result = await _userRepository.UpdateTwoFactorCodeAsync(user, code, expiration);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update two-factor code for user {Username}: {Errors}",
                    request.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                return new SendTwoFactorCodeResponse
                {
                    IsCodeSent = false,
                    Message = "Failed to generate verification code"
                };
            }

            // Send code via email
            await _emailService.SendTwoFactorCodeAsync(user.Email!, user.UserName!, code);

            // Log successful 2FA code sending
            await _activityLogService.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.TwoFactorCodeSent,
                description: $"Two-factor authentication code sent to user: {user.UserName}",
                metadata: new { Username = request.Username, Email = user.Email, ExpiresAt = expiration },
                cancellationToken: cancellationToken);

            // TEMPORARY: Log 2FA code for testing purposes
            _logger.LogInformation("TESTING: Two-factor code for {Username} is: {Code}", request.Username, code);
            _logger.LogInformation("Two-factor code sent to user {Username}", request.Username);
            return new SendTwoFactorCodeResponse
            {
                IsCodeSent = true,
                Message = "Verification code sent to your email address",
                CodeExpiresAt = expiration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending two-factor code to user {Username}", request.Username);
            return new SendTwoFactorCodeResponse
            {
                IsCodeSent = false,
                Message = "An error occurred while sending the verification code"
            };
        }
    }
}
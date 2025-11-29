using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.TwoFactor.Commands;

public class VerifyTwoFactorCodeCommandHandler : IRequestHandler<VerifyTwoFactorCodeCommand, VerifyTwoFactorCodeResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ILogger<VerifyTwoFactorCodeCommandHandler> _logger;

    public VerifyTwoFactorCodeCommandHandler(
        IIdentityExtension identityExtension,
        ITwoFactorService twoFactorService,
        ILogger<VerifyTwoFactorCodeCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _twoFactorService = twoFactorService;
        _logger = logger;
    }

    public async Task<VerifyTwoFactorCodeResponse> Handle(VerifyTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _identityExtension.FindByUserNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found", request.Username);
                return new VerifyTwoFactorCodeResponse
                {
                    IsVerified = false,
                    Message = "Invalid credentials"
                };
            }

            if (!user.IsTwoFactorEnabled)
            {
                _logger.LogWarning("Two-factor authentication is not enabled for user {Username}", request.Username);
                return new VerifyTwoFactorCodeResponse
                {
                    IsVerified = false,
                    Message = "Two-factor authentication is not enabled"
                };
            }

            // Validate the two-factor code
            var isValidCode = _twoFactorService.ValidateTwoFactorCode(
                user.TwoFactorCode,
                request.Code,
                user.TwoFactorCodeExpiration);

            if (!isValidCode)
            {
                _logger.LogWarning("Invalid or expired two-factor code for user {Username}", request.Username);
                return new VerifyTwoFactorCodeResponse
                {
                    IsVerified = false,
                    Message = "Invalid or expired verification code"
                };
            }

            // Clear the two-factor code after successful verification
            var clearResult = await _identityExtension.UpdateTwoFactorCodeAsync(user, string.Empty, JamaicaTimeHelper.Now);
            if (!clearResult.Succeeded)
            {
                _logger.LogWarning("Failed to clear two-factor code for user {Username}", request.Username);
            }

            // Update last login timestamp
            user.LastLoginAt = JamaicaTimeHelper.Now;
            await _identityExtension.UpdateUserAsync(user);

            // Generate access token
            var token = _identityExtension.GenerateToken(user.UserName!, user.Email!, user.Id);

            _logger.LogInformation("Two-factor authentication successful for user {Username}", request.Username);
            return new VerifyTwoFactorCodeResponse
            {
                IsVerified = true,
                Message = "Two-factor authentication successful",
                AccessToken = token,
                VerifiedAt = JamaicaTimeHelper.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying two-factor code for user {Username}", request.Username);
            return new VerifyTwoFactorCodeResponse
            {
                IsVerified = false,
                Message = "An error occurred during verification"
            };
        }
    }
}
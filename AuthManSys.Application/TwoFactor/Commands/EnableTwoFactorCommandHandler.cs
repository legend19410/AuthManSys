using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.TwoFactor.Commands;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        IIdentityExtension identityExtension,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _logger = logger;
    }

    public async Task<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _identityExtension.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return new EnableTwoFactorResponse
                {
                    IsEnabled = false,
                    Message = $"User with ID '{request.UserId}' not found"
                };
            }

            var result = await _identityExtension.EnableTwoFactorAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Two-factor authentication enabled for user {UserId}", request.UserId);
                return new EnableTwoFactorResponse
                {
                    IsEnabled = true,
                    Message = "Two-factor authentication has been enabled successfully",
                    UserId = request.UserId,
                    EnabledAt = DateTime.UtcNow
                };
            }

            _logger.LogWarning("Failed to enable two-factor authentication for user {UserId}: {Errors}",
                request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));

            return new EnableTwoFactorResponse
            {
                IsEnabled = false,
                Message = $"Failed to enable two-factor authentication: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling two-factor authentication for user {UserId}", request.UserId);
            return new EnableTwoFactorResponse
            {
                IsEnabled = false,
                Message = "An error occurred while enabling two-factor authentication"
            };
        }
    }
}
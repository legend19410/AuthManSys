using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Auth.TwoFactor.Commands;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        IUserRepository userRepository,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUserIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return new EnableTwoFactorResponse
                {
                    IsEnabled = false,
                    Message = $"User with ID '{request.UserId}' not found"
                };
            }

            var result = request.Enable
                ? await _userRepository.EnableTwoFactorAsync(user)
                : await _userRepository.DisableTwoFactorAsync(user);

            if (result.Succeeded)
            {
                var action = request.Enable ? "enabled" : "disabled";
                _logger.LogInformation("Two-factor authentication {Action} for user {UserId}", action, request.UserId);
                return new EnableTwoFactorResponse
                {
                    IsEnabled = request.Enable,
                    Message = $"Two-factor authentication has been {action} successfully",
                    UserId = request.UserId,
                    EnabledAt = request.Enable ? JamaicaTimeHelper.Now : null
                };
            }

            var actionFailed = request.Enable ? "enable" : "disable";
            _logger.LogWarning("Failed to {Action} two-factor authentication for user {UserId}: {Errors}",
                actionFailed, request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));

            return new EnableTwoFactorResponse
            {
                IsEnabled = !request.Enable,
                Message = $"Failed to {actionFailed} two-factor authentication: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }
        catch (Exception ex)
        {
            var action = request.Enable ? "enabling" : "disabling";
            _logger.LogError(ex, "Error {Action} two-factor authentication for user {UserId}", action, request.UserId);
            return new EnableTwoFactorResponse
            {
                IsEnabled = !request.Enable,
                Message = $"An error occurred while {action} two-factor authentication"
            };
        }
    }
}
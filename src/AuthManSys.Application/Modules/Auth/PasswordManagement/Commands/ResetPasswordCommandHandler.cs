using MediatR;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public ResetPasswordCommandHandler(IUserRepository userRepository, IActivityLogRepository activityLogService)
    {
        _userRepository = userRepository;
        _activityLogRepository = activityLogService;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Log failed password reset attempt for invalid email
                await _activityLogRepository.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.PasswordResetFailed,
                    description: $"Password reset failed - Invalid email: {request.Email}",
                    metadata: new { Email = request.Email, Reason = "UserNotFound" },
                    cancellationToken: cancellationToken);

                return new ResetPasswordResponse(false, "Invalid reset token or email.");
            }

            // Reset password using the token
            var result = await _userRepository.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // Log successful password reset
                await _activityLogRepository.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.PasswordResetSuccess,
                    description: $"Password reset successfully for user: {user.UserName}",
                    metadata: new { Email = request.Email, Username = user.UserName },
                    cancellationToken: cancellationToken);

                return new ResetPasswordResponse(true, "Password has been reset successfully.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            // Log failed password reset due to validation errors or invalid token
            await _activityLogRepository.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.PasswordResetFailed,
                description: $"Password reset failed for user: {user.UserName} - {errors}",
                metadata: new { Email = request.Email, Username = user.UserName, Errors = errors, Reason = "ValidationErrors" },
                cancellationToken: cancellationToken);

            return new ResetPasswordResponse(false, $"Failed to reset password: {errors}");
        }
        catch (Exception ex)
        {
            return new ResetPasswordResponse(false, $"An error occurred while resetting password: {ex.Message}");
        }
    }
}
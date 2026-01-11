using MediatR;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IActivityLogRepository activityLogService)
    {
        _userRepository = userRepository;
        _activityLogRepository = activityLogService;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.FindByUserNameAsync(request.Username);
            if (user == null)
            {
                return new ChangePasswordResponse(false, "User not found.");
            }

            // Verify current password
            var isCurrentPasswordValid = await _userRepository.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                // Log failed password change attempt
                await _activityLogRepository.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.PasswordResetFailed,
                    description: $"Password change failed for user: {user.UserName} - Invalid current password",
                    metadata: new { Username = request.Username, Reason = "InvalidCurrentPassword" },
                    cancellationToken: cancellationToken);

                return new ChangePasswordResponse(false, "Current password is incorrect.");
            }

            // Change password
            var result = await _userRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                // Log successful password change
                await _activityLogRepository.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.PasswordChanged,
                    description: $"Password changed successfully for user: {user.UserName}",
                    metadata: new { Username = request.Username },
                    cancellationToken: cancellationToken);

                return new ChangePasswordResponse(true, "Password changed successfully.");
            }

            var errors = string.Join(", ", result.Errors);

            // Log failed password change due to validation errors
            await _activityLogRepository.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.PasswordResetFailed,
                description: $"Password change failed for user: {user.UserName} - Validation errors",
                metadata: new { Username = request.Username, Errors = errors, Reason = "ValidationErrors" },
                cancellationToken: cancellationToken);

            return new ChangePasswordResponse(false, $"Failed to change password: {errors}");
        }
        catch (Exception ex)
        {
            return new ChangePasswordResponse(false, $"An error occurred while changing password: {ex.Message}");
        }
    }
}
using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IActivityLogService activityLogService)
    {
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Log failed password reset attempt for invalid email
                await _activityLogService.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.PasswordResetFailed,
                    description: $"Password reset failed - Invalid email: {request.Email}",
                    metadata: new { Email = request.Email, Reason = "UserNotFound" },
                    cancellationToken: cancellationToken);

                return new ResetPasswordResponse(false, "Invalid reset token or email.");
            }

            // Reset password using the token
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // Log successful password reset
                await _activityLogService.LogActivityAsync(
                    userId: user.UserId,
                    eventType: ActivityEventType.PasswordResetSuccess,
                    description: $"Password reset successfully for user: {user.UserName}",
                    metadata: new { Email = request.Email, Username = user.UserName },
                    cancellationToken: cancellationToken);

                return new ResetPasswordResponse(true, "Password has been reset successfully.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            // Log failed password reset due to validation errors or invalid token
            await _activityLogService.LogActivityAsync(
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

using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;
using System.Web;

namespace AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IActivityLogRepository _activityLogRepository;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IActivityLogRepository activityLogService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _activityLogRepository = activityLogService;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Log failed password reset attempt for non-existent email
                await _activityLogRepository.LogActivityAsync(
                    userId: null,
                    eventType: ActivityEventType.PasswordResetRequested,
                    description: $"Password reset requested for non-existent email: {request.Email}",
                    metadata: new { Email = request.Email, Reason = "UserNotFound" },
                    cancellationToken: cancellationToken);

                // Return success even if user doesn't exist for security reasons
                return new ForgotPasswordResponse(true, "If an account with that email exists, a password reset link has been sent.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            // Create reset link (you can customize the URL as needed)
            var resetLink = $"https://yourdomain.com/reset-password?email={HttpUtility.UrlEncode(request.Email)}&token={encodedToken}";

            // Send email
            var emailSubject = "Password Reset Request";
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.FirstName ?? user.UserName},</p>
                <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request this password reset, please ignore this email.</p>
                <br>
                <p>Best regards,<br>AuthManSys Team</p>
            ";

            await _emailService.SendEmailAsync(request.Email, emailSubject, emailBody);

            // Log successful password reset request
            await _activityLogRepository.LogActivityAsync(
                userId: user.UserId,
                eventType: ActivityEventType.PasswordResetRequested,
                description: $"Password reset requested for user: {user.UserName}",
                metadata: new { Email = request.Email, Username = user.UserName },
                cancellationToken: cancellationToken);

            return new ForgotPasswordResponse(true, "If an account with that email exists, a password reset link has been sent.");
        }
        catch (Exception ex)
        {
            return new ForgotPasswordResponse(false, $"An error occurred while processing the password reset request: {ex.Message}");
        }
    }
}
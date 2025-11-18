namespace AuthManSys.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string username, string confirmationToken);
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
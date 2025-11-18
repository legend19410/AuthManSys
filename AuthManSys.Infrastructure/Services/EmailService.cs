using System.Net;
using System.Net.Mail;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace AuthManSys.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string username, string confirmationToken)
    {
        var subject = "Confirm your email address - AuthManSys";
        var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Email Confirmation</h2>
                        <p>Hello {username},</p>
                        <p>Thank you for registering with AuthManSys. To complete your registration and activate your account, please confirm your email address by clicking the button below:</p>

                        <div style='margin: 30px 0; text-align: center;'>
                            <a href='#' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Confirm Email Address
                            </a>
                        </div>

                        <p>If the button doesn't work, you can also use this confirmation token manually:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; margin: 20px 0; border-left: 4px solid #3498db; font-family: monospace; word-break: break-all;'>
                            {confirmationToken}
                        </div>

                        <p style='margin-top: 30px; font-size: 14px; color: #666;'>
                            If you did not create an account with us, please ignore this email.
                        </p>

                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #999; text-align: center;'>
                            This email was sent by AuthManSys. Please do not reply to this email.
                        </p>
                    </div>
                </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken)
    {
        var subject = "Password Reset Request - AuthManSys";
        var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>Password Reset Request</h2>
                        <p>Hello {username},</p>
                        <p>We received a request to reset your password. If you made this request, use the token below to reset your password:</p>

                        <div style='background-color: #f8f9fa; padding: 15px; margin: 20px 0; border-left: 4px solid #e74c3c; font-family: monospace; word-break: break-all;'>
                            {resetToken}
                        </div>

                        <p style='color: #e74c3c; font-weight: bold;'>
                            This token will expire in 24 hours for security reasons.
                        </p>

                        <p style='margin-top: 30px; font-size: 14px; color: #666;'>
                            If you did not request a password reset, please ignore this email. Your password will remain unchanged.
                        </p>

                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #999; text-align: center;'>
                            This email was sent by AuthManSys. Please do not reply to this email.
                        </p>
                    </div>
                </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_emailSettings.FromEmail) || string.IsNullOrEmpty(_emailSettings.Username))
        {
            throw new InvalidOperationException("Email settings are not configured properly. Please check the EmailSettings configuration.");
        }

        using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
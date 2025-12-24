using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.UserEmail.Commands;

public class SendConfirmationEmailCommandHandler : IRequestHandler<SendConfirmationEmailCommand, SendEmailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public SendConfirmationEmailCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<SendEmailResponse> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return new SendEmailResponse
            {
                IsEmailSent = false,
                Message = "User not found",
                Email = ""
            };
        }

        // Check if email is already confirmed
        if (await _userRepository.IsEmailConfirmedAsync(user))
        {
            return new SendEmailResponse
            {
                IsEmailSent = false,
                Message = "Email is already confirmed",
                Email = user.Email ?? ""
            };
        }

        try
        {
            // Generate email confirmation token
            var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);

            // Send the confirmation email
            await _emailService.SendEmailConfirmationAsync(
                user.Email ?? "",
                user.UserName ?? request.Username,
                token);

            return new SendEmailResponse
            {
                IsEmailSent = true,
                Message = "Confirmation email sent successfully",
                Email = user.Email ?? ""
            };
        }
        catch (Exception ex)
        {
            return new SendEmailResponse
            {
                IsEmailSent = false,
                Message = $"Failed to send confirmation email: {ex.Message}",
                Email = user.Email ?? ""
            };
        }
    }
}
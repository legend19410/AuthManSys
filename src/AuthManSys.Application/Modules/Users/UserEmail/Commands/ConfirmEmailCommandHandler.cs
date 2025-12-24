using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.UserEmail.Commands;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProvider _identityProvider;

    public ConfirmEmailCommandHandler(
        IUserRepository userRepository,
        IIdentityProvider identityProvider)
    {
        _userRepository = userRepository;
        _identityProvider = identityProvider;
    }

    public async Task<ConfirmEmailResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return new ConfirmEmailResponse
            {
                IsConfirmed = false,
                Message = "User not found",
                Username = request.Username,
                Email = ""
            };
        }

        // Check if email is already confirmed
        if (await _userRepository.IsEmailConfirmedAsync(user))
        {
            return new ConfirmEmailResponse
            {
                IsConfirmed = true,
                Message = "Email is already confirmed",
                Username = user.UserName ?? request.Username,
                Email = user.Email ?? ""
            };
        }

        // Confirm the email with the provided token
        var result = await _userRepository.ConfirmEmailAsync(user, request.Token);

        if (result.Succeeded)
        {
            return new ConfirmEmailResponse
            {
                IsConfirmed = true,
                Message = "Email confirmed successfully",
                Username = user.UserName ?? request.Username,
                Email = user.Email ?? ""
            };
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return new ConfirmEmailResponse
        {
            IsConfirmed = false,
            Message = $"Email confirmation failed: {errors}",
            Username = user.UserName ?? request.Username,
            Email = user.Email ?? ""
        };
    }
}
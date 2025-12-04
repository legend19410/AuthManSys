using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UserEmail.Commands;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResponse>
{
    private readonly IIdentityService _identityExtension;

    public ConfirmEmailCommandHandler(IIdentityService identityExtension)
    {
        _identityExtension = identityExtension;
    }

    public async Task<ConfirmEmailResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _identityExtension.FindByUserNameAsync(request.Username);
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
        if (await _identityExtension.IsEmailConfirmedAsync(request.Username))
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
        var result = await _identityExtension.ConfirmEmailAsync(request.Username, request.Token);

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
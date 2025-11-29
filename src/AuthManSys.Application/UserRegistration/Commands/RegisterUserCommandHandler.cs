using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;

namespace AuthManSys.Application.UserRegistration.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IIdentityExtension _identityExtension;

    public RegisterUserCommandHandler(IIdentityExtension identityExtension)
    {
        _identityExtension = identityExtension;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _identityExtension.FindByUserNameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        var existingEmailUser = await _identityExtension.FindByEmailAsync(request.Email);
        if (existingEmailUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create user
        var result = await _identityExtension.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Get the created user
        var createdUser = await _identityExtension.FindByUserNameAsync(request.Username);
        if (createdUser == null)
        {
            throw new InvalidOperationException("Failed to retrieve created user");
        }

        // Assign default role (User)
        await _identityExtension.AddToRoleAsync(createdUser, "User");

        return new RegisterResponse
        {
            UserId = createdUser.Id,
            Username = createdUser.UserName ?? request.Username,
            Email = createdUser.Email ?? request.Email,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            IsEmailConfirmed = createdUser.EmailConfirmed,
            Message = "User registered successfully"
        };
    }
}
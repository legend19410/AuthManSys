using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;

namespace AuthManSys.Application.Modules.Auth.UserRegistration.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProvider _identityProvider;

    public RegisterUserCommandHandler(IUserRepository userRepository, IIdentityProvider identityProvider)
    {
        _userRepository = userRepository;
        _identityProvider = identityProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Create user using IdentityProvider
        var result = await _identityProvider.CreateUserAsync(
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
        var createdUser = await _userRepository.GetUserInformationByUsernameAsync(request.Username);
        if (createdUser == null)
        {
            throw new InvalidOperationException("Failed to retrieve created user");
        }

        // Get user entity for role assignment
        var userEntity = await _userRepository.GetByUsernameAsync(request.Username);
        if (userEntity != null)
        {
            // Assign default role (User)
            await _userRepository.AddToRoleAsync(userEntity, "User", null);
        }

        return new RegisterResponse
        {
            UserId = createdUser.Id.ToString(),
            Username = createdUser.Username ?? request.Username,
            Email = createdUser.Email ?? request.Email,
            FirstName = createdUser.FirstName ?? "",
            LastName = createdUser.LastName ?? "",
            IsEmailConfirmed = false, // New users require email confirmation by default
            Message = "User registered successfully"
        };
    }
}
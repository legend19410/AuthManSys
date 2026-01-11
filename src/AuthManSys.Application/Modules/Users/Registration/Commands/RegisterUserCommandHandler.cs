using MediatR;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;

namespace AuthManSys.Application.Modules.Users.Registration.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Create user using UserRepository
        var result = await _userRepository.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors);
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
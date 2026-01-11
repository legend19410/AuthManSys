using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.UpdateUser.Commands;

public class UpdateUserInformationCommandHandler : IRequestHandler<UpdateUserInformationCommand, UpdateUserInformationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserInformationCommandHandler> _logger;

    public UpdateUserInformationCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserInformationCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UpdateUserInformationResponse> Handle(UpdateUserInformationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the user first
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found", request.Username);
                return new UpdateUserInformationResponse
                {
                    IsUpdated = false,
                    Message = "User not found"
                };
            }

            // Update user information
            user.FirstName = request.FirstName ?? string.Empty;
            user.LastName = request.LastName ?? string.Empty;
            user.Email = request.Email ?? string.Empty;
            user.NormalizedEmail = request.Email?.ToUpperInvariant() ?? string.Empty;

            var result = await _userRepository.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} information updated successfully", request.Username);
                return new UpdateUserInformationResponse
                {
                    IsUpdated = true,
                    Message = "User information updated successfully",
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                };
            }

            _logger.LogWarning("Failed to update user {Username}: {Errors}", request.Username, string.Join(", ", result.Errors));
            return new UpdateUserInformationResponse
            {
                IsUpdated = false,
                Message = $"Failed to update user: {string.Join(", ", result.Errors)}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user information for {Username}", request.Username);
            return new UpdateUserInformationResponse
            {
                IsUpdated = false,
                Message = "An error occurred while updating user information"
            };
        }
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UpdateUser.Commands;

public class UpdateUserInformationCommandHandler : IRequestHandler<UpdateUserInformationCommand, UpdateUserInformationResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ILogger<UpdateUserInformationCommandHandler> _logger;

    public UpdateUserInformationCommandHandler(
        IIdentityExtension identityExtension,
        ILogger<UpdateUserInformationCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _logger = logger;
    }

    public async Task<UpdateUserInformationResponse> Handle(UpdateUserInformationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _identityExtension.FindByUserNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found for update", request.Username);
                return new UpdateUserInformationResponse
                {
                    IsUpdated = false,
                    Message = "User not found"
                };
            }

            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName)
            {
                user.FirstName = request.FirstName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName)
            {
                user.LastName = request.LastName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
            {
                var existingUser = await _identityExtension.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _logger.LogWarning("Email {Email} is already in use by another user", request.Email);
                    return new UpdateUserInformationResponse
                    {
                        IsUpdated = false,
                        Message = "Email address is already in use"
                    };
                }

                user.Email = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.UserName = request.Email;
                user.NormalizedUserName = request.Email.ToUpper();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return new UpdateUserInformationResponse
                {
                    IsUpdated = true,
                    Message = "No changes detected",
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
            }

            var result = await _identityExtension.UpdateUserAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} information updated successfully", request.Username);
                return new UpdateUserInformationResponse
                {
                    IsUpdated = true,
                    Message = "User information updated successfully",
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
            }

            _logger.LogWarning("Failed to update user {Username}: {Errors}", request.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new UpdateUserInformationResponse
            {
                IsUpdated = false,
                Message = $"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
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
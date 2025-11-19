using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.UpdateUser.Commands;

public class PatchUserInformationCommandHandler : IRequestHandler<PatchUserInformationCommand, UpdateUserInformationResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ILogger<PatchUserInformationCommandHandler> _logger;

    public PatchUserInformationCommandHandler(
        IIdentityExtension identityExtension,
        ILogger<PatchUserInformationCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _logger = logger;
    }

    public async Task<UpdateUserInformationResponse> Handle(PatchUserInformationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _identityExtension.FindByUserNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found for patch update", request.Username);
                return new UpdateUserInformationResponse
                {
                    IsUpdated = false,
                    Message = "User not found"
                };
            }

            bool hasChanges = false;

            // Only update fields that were specifically provided
            if (request.UpdateFirstName && !string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName)
            {
                user.FirstName = request.FirstName;
                hasChanges = true;
            }

            if (request.UpdateLastName && !string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName)
            {
                user.LastName = request.LastName;
                hasChanges = true;
            }

            if (request.UpdateEmail && !string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
            {
                var existingUser = await _identityExtension.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _logger.LogWarning("Email {Email} is already in use by another user during patch update", request.Email);
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
                    Message = "No changes detected or no fields provided for update",
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
            }

            var result = await _identityExtension.UpdateUserAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} information patched successfully", request.Username);
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

            _logger.LogWarning("Failed to patch update user {Username}: {Errors}", request.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new UpdateUserInformationResponse
            {
                IsUpdated = false,
                Message = $"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patch updating user information for {Username}", request.Username);
            return new UpdateUserInformationResponse
            {
                IsUpdated = false,
                Message = "An error occurred while updating user information"
            };
        }
    }
}
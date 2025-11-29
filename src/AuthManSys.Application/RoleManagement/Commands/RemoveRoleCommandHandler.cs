using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.RoleManagement.Commands;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, RemoveRoleResponse>
{
    private readonly IIdentityExtension _identityExtension;
    private readonly ILogger<RemoveRoleCommandHandler> _logger;

    public RemoveRoleCommandHandler(
        IIdentityExtension identityExtension,
        ILogger<RemoveRoleCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _logger = logger;
    }

    public async Task<RemoveRoleResponse> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await _identityExtension.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return new RemoveRoleResponse
                {
                    IsRemoved = false,
                    Message = $"User with ID '{request.UserId}' not found"
                };
            }

            // Check if role exists
            if (!await _identityExtension.RoleExistsAsync(request.RoleName))
            {
                _logger.LogWarning("Role {RoleName} does not exist", request.RoleName);
                return new RemoveRoleResponse
                {
                    IsRemoved = false,
                    Message = $"Role '{request.RoleName}' does not exist"
                };
            }

            // Check if user has the role
            var userRoles = await _identityExtension.GetUserRolesAsync(user);
            if (!userRoles.Contains(request.RoleName))
            {
                _logger.LogWarning("User {UserId} does not have role {RoleName}", request.UserId, request.RoleName);
                return new RemoveRoleResponse
                {
                    IsRemoved = false,
                    Message = $"User does not have the role '{request.RoleName}'"
                };
            }

            var result = await _identityExtension.RemoveFromRoleAsync(user, request.RoleName);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} removed from user {UserId} successfully", request.RoleName, request.UserId);
                return new RemoveRoleResponse
                {
                    IsRemoved = true,
                    Message = "Role removed successfully",
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    RemovedAt = JamaicaTimeHelper.Now
                };
            }

            _logger.LogWarning("Failed to remove role {RoleName} from user {UserId}: {Errors}", request.RoleName, request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new RemoveRoleResponse
            {
                IsRemoved = false,
                Message = $"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", request.RoleName, request.UserId);
            return new RemoveRoleResponse
            {
                IsRemoved = false,
                Message = "An error occurred while removing the role"
            };
        }
    }
}
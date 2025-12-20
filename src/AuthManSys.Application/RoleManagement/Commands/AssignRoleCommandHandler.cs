using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.RoleManagement.Commands;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, AssignRoleResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AssignRoleCommandHandler> _logger;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<AssignRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<AssignRoleResponse> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await _userRepository.GetByUserIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return new AssignRoleResponse
                {
                    IsAssigned = false,
                    Message = $"User with ID '{request.UserId}' not found",
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    AssignedAt = null
                };
            }

            // Check if role exists
            if (!await _roleRepository.RoleExistsAsync(request.RoleName))
            {
                _logger.LogWarning("Role {RoleName} does not exist", request.RoleName);
                return new AssignRoleResponse
                {
                    IsAssigned = false,
                    Message = $"Role '{request.RoleName}' does not exist",
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    AssignedAt = null
                };
            }

            // Check if user already has the role
            var userRoles = await _userRepository.GetRolesAsync(user);
            if (userRoles.Contains(request.RoleName))
            {
                _logger.LogWarning("User {UserId} already has role {RoleName}", request.UserId, request.RoleName);
                return new AssignRoleResponse
                {
                    IsAssigned = false,
                    Message = $"User already has the role '{request.RoleName}'",
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    AssignedAt = null
                };
            }

            var result = await _userRepository.AddToRoleAsync(user, request.RoleName, request.AssignedBy);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} assigned to user {UserId} successfully", request.RoleName, request.UserId);
                return new AssignRoleResponse
                {
                    IsAssigned = true,
                    Message = "Role assigned successfully",
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    AssignedAt = JamaicaTimeHelper.Now
                };
            }

            _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Errors}", request.RoleName, request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new AssignRoleResponse
            {
                IsAssigned = false,
                Message = $"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                UserId = request.UserId,
                RoleName = request.RoleName,
                AssignedAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", request.RoleName, request.UserId);
            return new AssignRoleResponse
            {
                IsAssigned = false,
                Message = "An error occurred while assigning the role",
                UserId = request.UserId,
                RoleName = request.RoleName,
                AssignedAt = null
            };
        }
    }
}
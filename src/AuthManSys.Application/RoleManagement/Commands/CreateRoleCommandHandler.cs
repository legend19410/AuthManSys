using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.RoleManagement.Commands;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IIdentityService _identityExtension;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        IIdentityService identityExtension,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _identityExtension = identityExtension;
        _logger = logger;
    }

    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if role already exists
            if (await _identityExtension.RoleExistsAsync(request.RoleName))
            {
                _logger.LogWarning("Role {RoleName} already exists", request.RoleName);
                return new CreateRoleResponse
                {
                    IsCreated = false,
                    Message = $"Role '{request.RoleName}' already exists"
                };
            }

            var result = await _identityExtension.CreateRoleAsync(request.RoleName, request.Description);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} created successfully", request.RoleName);
                return new CreateRoleResponse
                {
                    IsCreated = true,
                    Message = "Role created successfully",
                    RoleName = request.RoleName,
                    Description = request.Description,
                    CreatedAt = JamaicaTimeHelper.Now
                };
            }

            _logger.LogWarning("Failed to create role {RoleName}: {Errors}", request.RoleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new CreateRoleResponse
            {
                IsCreated = false,
                Message = $"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", request.RoleName);
            return new CreateRoleResponse
            {
                IsCreated = false,
                Message = "An error occurred while creating the role"
            };
        }
    }
}
using AuthManSys.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace AuthManSys.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionService permissionService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        _logger.LogDebug("PermissionAuthorizationHandler: Checking permission {Permission}", requirement.Permission);
        _logger.LogDebug("User identity: IsAuthenticated={IsAuthenticated}, Name={Name}",
            user?.Identity?.IsAuthenticated, user?.Identity?.Name);

        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is not authenticated for permission: {Permission}. IsAuthenticated: {IsAuthenticated}",
                requirement.Permission, user?.Identity?.IsAuthenticated);
            context.Fail();
            return;
        }

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims for permission: {Permission}", requirement.Permission);
            context.Fail();
            return;
        }

        _logger.LogDebug("Found userId {UserId} for permission check {Permission}", userId, requirement.Permission);

        try
        {
            var hasPermission = await _permissionService.UserHasPermissionAsync(userId, requirement.Permission);
            _logger.LogDebug("Permission check result: {HasPermission} for user {UserId} and permission {Permission}",
                hasPermission, userId, requirement.Permission);

            if (hasPermission)
            {
                _logger.LogDebug("User {UserId} granted access to permission: {Permission}", userId, requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} denied access to permission: {Permission}", userId, requirement.Permission);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", requirement.Permission, userId);
            context.Fail();
        }
    }
}
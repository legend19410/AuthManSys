using AuthManSys.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IPermissionService permissionService,
        ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ViewPermissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get role-permission mappings for admin UI
    /// </summary>
    [HttpGet("role-mappings")]
    [Authorize(Policy = "ViewPermissions")]
    public async Task<IActionResult> GetRolePermissionMappings()
    {
        try
        {
            var mappings = await _permissionService.GetRolePermissionMappingsAsync();
            return Ok(mappings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role-permission mappings");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Grant permission to role
    /// </summary>
    [HttpPost("grant")]
    [Authorize(Policy = "GrantPermissions")]
    public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionRequest request)
    {
        try
        {
            var currentUser = User.Identity?.Name;
            await _permissionService.GrantPermissionToRoleAsync(
                request.RoleId,
                request.PermissionName,
                currentUser);

            _logger.LogInformation("Permission {Permission} granted to role {RoleId} by {User}",
                request.PermissionName, request.RoleId, currentUser);

            return Ok(new { message = "Permission granted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Revoke permission from role
    /// </summary>
    [HttpPost("revoke")]
    [Authorize(Policy = "RevokePermissions")]
    public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionRequest request)
    {
        try
        {
            await _permissionService.RevokePermissionFromRoleAsync(
                request.RoleId,
                request.PermissionName);

            _logger.LogInformation("Permission {Permission} revoked from role {RoleId}",
                request.PermissionName, request.RoleId);

            return Ok(new { message = "Permission revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if current user has specific permission
    /// </summary>
    [HttpGet("check/{permissionName}")]
    public async Task<IActionResult> CheckPermission(string permissionName)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found");
            }

            var hasPermission = await _permissionService.UserHasPermissionAsync(userId, permissionName);
            return Ok(new { hasPermission });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get current user's permissions
    /// </summary>
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found");
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user permissions");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record
GrantPermissionRequest(string RoleId, string PermissionName);
public record RevokePermissionRequest(string RoleId, string PermissionName);
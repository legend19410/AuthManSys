using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AuthManSys.Api.Models;
using AuthManSys.Application.Security.Commands.Login;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.UserEmail.Commands;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IPermissionService permissionService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Login failed: {ex.Message}");
        }
    }

    [Authorize]
    [HttpGet("user-info")]
    public ActionResult GetUserInfo()
    {
        var username = User.FindFirst("username")?.Value;
        var email = User.FindFirst("email")?.Value;
        var role = User.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized("Invalid or missing token");
        }

        return Ok(new
        {
            Username = username,
            Email = email,
            Role = role,
            AuthenticatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet("permissions")]
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
    [HttpGet("permissions/role-mappings")]
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
    [HttpPost("permissions/grant")]
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
    [HttpPost("permissions/revoke")]
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
    [HttpGet("permissions/check/{permissionName}")]
    [Authorize]
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
    [HttpGet("permissions/my-permissions")]
    [Authorize]
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

    /// <summary>
    /// Confirm user email with token
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ConfirmEmailResponse>> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ConfirmEmailCommand(request.Username, request.Token);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsConfirmed)
            {
                _logger.LogInformation("Email confirmed successfully for user {Username}", request.Username);
                return Ok(result);
            }

            _logger.LogWarning("Email confirmation failed for user {Username}: {Message}", request.Username, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while confirming email for user {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred while confirming the email." });
        }
    }

    /// <summary>
    /// Send confirmation email to user
    /// </summary>
    [HttpPost("send-confirmation-email")]
    [AllowAnonymous]
    public async Task<ActionResult<SendEmailResponse>> SendConfirmationEmail(
        [FromBody] SendConfirmationEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new SendConfirmationEmailCommand(request.Username);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsEmailSent)
            {
                _logger.LogInformation("Confirmation email sent successfully to user {Username}", request.Username);
                return Ok(result);
            }

            _logger.LogWarning("Failed to send confirmation email for user {Username}: {Message}", request.Username, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending confirmation email for user {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred while sending confirmation email." });
        }
    }
}

public record GrantPermissionRequest(string RoleId, string PermissionName);
public record RevokePermissionRequest(string RoleId, string PermissionName);
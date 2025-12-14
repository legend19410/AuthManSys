
using AuthManSys.Application.Login.Commands;
using AuthManSys.Application.RefreshToken.Commands;
using AuthManSys.Application.UserEmail.Commands;
using AuthManSys.Application.RoleManagement.Commands;
using AuthManSys.Application.RoleManagement.Queries;
using AuthManSys.Application.TwoFactor.Commands;
using AuthManSys.Application.GoogleAuth.Commands;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Exceptions;
using AuthManSys.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AuthController> _logger;
    private readonly IIdentityService _identityService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IGoogleTokenService _googleTokenService;

    public AuthController(
        IMediator mediator,
        IPermissionService permissionService,
        ILogger<AuthController> logger,
        IIdentityService identityService,
        SignInManager<ApplicationUser> signInManager,
        IGoogleTokenService googleTokenService)
    {
        _mediator = mediator;
        _permissionService = permissionService;
        _logger = logger;
        _identityService = identityService;
        _signInManager = signInManager;
        _googleTokenService = googleTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password, request.RememberMe);
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("google-token-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> GoogleTokenLogin([FromBody] GoogleTokenRequest request)
    {
        var command = new GoogleTokenLoginCommand(request.IdToken, request.Username);
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var response = await _mediator.Send(command);

        if (response.IsSuccess)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet("permissions")]
    [Authorize(Policy = "ViewPermissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsDetailedAsync();
        return Ok(permissions);
    }

    /// <summary>
    /// Get role-permission mappings for admin UI
    /// </summary>
    [HttpGet("permissions/role-mappings")]
    [Authorize(Policy = "ViewPermissions")]
    public async Task<IActionResult> GetRolePermissionMappings()
    {
        var mappings = await _permissionService.GetDetailedRolePermissionMappingsAsync();
        return Ok(mappings);
    }

    /// <summary>
    /// Grant permission to role
    /// </summary>
    [HttpPost("permissions/grant")]
    [Authorize(Policy = "GrantPermissions")]
    public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionRequest request)
    {
        var currentUser = User.Identity?.Name;
        var wasGranted = await _permissionService.GrantPermissionToRoleByNameAsync(
            request.RoleName,
            request.PermissionName,
            currentUser);

        if (wasGranted)
        {
            _logger.LogInformation("Permission {Permission} granted to role {RoleName} by {User}",
                request.PermissionName, request.RoleName, currentUser);

            return Ok(new { message = "Permission granted successfully" });
        }
        else
        {
            _logger.LogInformation("Permission {Permission} was already assigned to role {RoleName}",
                request.PermissionName, request.RoleName);

            return Ok(new { message = "Permission was already assigned to this role" });
        }
    }

    /// <summary>
    /// Revoke permission from role
    /// </summary>
    [HttpPost("permissions/revoke")]
    [Authorize(Policy = "RevokePermissions")]
    public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionRequest request)
    {
        var wasRevoked = await _permissionService.RevokePermissionFromRoleByNameAsync(
            request.RoleName,
            request.PermissionName);

        if (wasRevoked)
        {
            _logger.LogInformation("Permission {Permission} revoked from role {RoleName}",
                request.PermissionName, request.RoleName);

            return Ok(new { message = "Permission revoked successfully" });
        }
        else
        {
            _logger.LogInformation("Permission {Permission} was not assigned to role {RoleName}",
                request.PermissionName, request.RoleName);

            return Ok(new { message = "Permission was not assigned to this role" });
        }
    }

    /// <summary>
    /// Check if current user has specific permission
    /// </summary>
    [HttpGet("permissions/check/{permissionName}")]
    [Authorize]
    public async Task<IActionResult> CheckPermission(string permissionName)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID not found");
        }

        var hasPermission = await _permissionService.UserHasPermissionAsync(userId, permissionName);
        return Ok(new { hasPermission });
    }

    /// <summary>
    /// Get current user's permissions
    /// </summary>
    [HttpGet("permissions/my-permissions")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID not found");
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
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

    /// <summary>
    /// Send confirmation email to user
    /// </summary>
    [HttpPost("send-confirmation-email")]
    [AllowAnonymous]
    public async Task<ActionResult<SendEmailResponse>> SendConfirmationEmail(
        [FromBody] SendConfirmationEmailRequest request,
        CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Get all available roles in the system
    /// </summary>
    /// <returns>
    /// Returns a list of all roles configured in the database.
    /// Each role contains:
    /// - Id: Unique identifier for the role
    /// - Name: The role name (e.g., "Admin", "User", "Manager")
    /// - NormalizedName: The uppercase normalized version of the role name
    /// </returns>
    /// <response code="200">Successfully retrieved all roles</response>
    /// <response code="401">Unauthorized - valid authentication token required</response>
    /// <response code="400">Bad Request - an error occurred while retrieving roles</response>
    [HttpGet("roles")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles(CancellationToken cancellationToken = default)
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost("roles/create")]
    [Authorize]
    public async Task<ActionResult<CreateRoleResponse>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateRoleCommand(request.RoleName, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsCreated)
        {
            _logger.LogInformation("Role {RoleName} created successfully", request.RoleName);
            return Ok(result);
        }

        _logger.LogWarning("Failed to create role {RoleName}: {Message}", request.RoleName, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("roles/assign")]
    [Authorize]
    public async Task<ActionResult<AssignRoleResponse>> AssignRole(
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AssignRoleCommand(request.UserId, request.RoleName);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsAssigned)
        {
            _logger.LogInformation("Role {RoleName} assigned to user {UserId} successfully", request.RoleName, request.UserId);
            return Ok(result);
        }

        _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Message}", request.RoleName, request.UserId, result.Message);
        return Ok(result);
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpPost("roles/remove")]
    [Authorize]
    public async Task<ActionResult<RemoveRoleResponse>> RemoveRole(
        [FromBody] RemoveRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveRoleCommand(request.UserId, request.RoleName);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsRemoved)
        {
            _logger.LogInformation("Role {RoleName} removed from user {UserId} successfully", request.RoleName, request.UserId);
            return Ok(result);
        }

        _logger.LogWarning("Failed to remove role {RoleName} from user {UserId}: {Message}", request.RoleName, request.UserId, result.Message);
        return Ok(result);
    }

    /// <summary>
    /// Enable or disable two-factor authentication for a user
    /// </summary>
    [HttpPost("two-factor/enable")]
   // [Authorize]
    public async Task<ActionResult<EnableTwoFactorResponse>> EnableTwoFactor(
        [FromBody] EnableTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new EnableTwoFactorCommand(request.UserId, request.Enable);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsEnabled == request.Enable)
        {
            var action = request.Enable ? "enabled" : "disabled";
            _logger.LogInformation("Two-factor authentication {Action} for user {UserId}", action, request.UserId);
            return Ok(result);
        }

        var actionFailed = request.Enable ? "enable" : "disable";
        _logger.LogWarning("Failed to {Action} two-factor authentication for user {UserId}: {Message}", actionFailed, request.UserId, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Send two-factor authentication code via email
    /// </summary>
    [HttpPost("two-factor/send-code")]
    [AllowAnonymous]
    public async Task<ActionResult<SendTwoFactorCodeResponse>> SendTwoFactorCode(
        [FromBody] SendTwoFactorCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SendTwoFactorCodeCommand(request.Username);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsCodeSent)
        {
            _logger.LogInformation("Two-factor code sent for user {Username}", request.Username);
            return Ok(result);
        }

        _logger.LogWarning("Failed to send two-factor code for user {Username}: {Message}", request.Username, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Verify two-factor authentication code and complete login
    /// </summary>
    [HttpPost("two-factor/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyTwoFactorCodeResponse>> VerifyTwoFactorCode(
        [FromBody] VerifyTwoFactorCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new VerifyTwoFactorCodeCommand(request.Username, request.Code);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsVerified)
        {
            _logger.LogInformation("Two-factor verification successful for user {Username}", request.Username);
            return Ok(result);
        }

        _logger.LogWarning("Two-factor verification failed for user {Username}: {Message}", request.Username, result.Message);
        return BadRequest(result);
    }

    /// <summary>
    /// Bulk grant permissions to multiple roles
    /// </summary>
    [HttpPost("permissions/bulk-grant")]
    [Authorize(Policy = "GrantPermissions")]
    public async Task<IActionResult> BulkGrantPermissions([FromBody] BulkPermissionAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data" });
        }

        var currentUser = User.Identity?.Name;

        // Convert API model to service model
        var permissions = request.Permissions.Select(p => new Application.Common.Models.RolePermissionMappingRequest
        {
            RoleName = p.RoleName,
            PermissionName = p.PermissionName
        }).ToList();

        var result = await _permissionService.BulkGrantPermissionsAsync(permissions, currentUser);

        _logger.LogInformation(
            "Bulk permission grant operation completed by {User}: {Total} total, {Success} successful, {Skipped} skipped, {Failed} failed",
            currentUser, result.TotalOperations, result.SuccessfulOperations, result.SkippedOperations, result.FailedOperations);

        if (result.IsFullySuccessful)
        {
            return Ok(new
            {
                message = $"Successfully granted {result.SuccessfulOperations} permissions",
                details = result
            });
        }
        else if (result.HasPartialSuccess)
        {
            return Ok(new
            {
                message = $"Partial success: {result.SuccessfulOperations} granted, {result.SkippedOperations} skipped, {result.FailedOperations} failed",
                details = result
            });
        }
        else
        {
            return BadRequest(new
            {
                message = $"Bulk grant failed: {string.Join(", ", result.ErrorMessages)}",
                details = result
            });
        }
    }

    /// <summary>
    /// Bulk revoke permissions from multiple roles
    /// </summary>
    [HttpPost("permissions/bulk-revoke")]
    [Authorize(Policy = "RevokePermissions")]
    public async Task<IActionResult> BulkRevokePermissions([FromBody] BulkPermissionRemovalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data" });
        }

        var currentUser = User.Identity?.Name;

        // Convert API model to service model
        var permissions = request.Permissions.Select(p => new Application.Common.Models.RolePermissionMappingRequest
        {
            RoleName = p.RoleName,
            PermissionName = p.PermissionName
        }).ToList();

        var result = await _permissionService.BulkRevokePermissionsAsync(permissions);

        _logger.LogInformation(
            "Bulk permission revoke operation completed by {User}: {Total} total, {Success} successful, {Skipped} skipped, {Failed} failed",
            currentUser, result.TotalOperations, result.SuccessfulOperations, result.SkippedOperations, result.FailedOperations);

        if (result.IsFullySuccessful)
        {
            return Ok(new
            {
                message = $"Successfully revoked {result.SuccessfulOperations} permissions",
                details = result
            });
        }
        else if (result.HasPartialSuccess)
        {
            return Ok(new
            {
                message = $"Partial success: {result.SuccessfulOperations} revoked, {result.SkippedOperations} skipped, {result.FailedOperations} failed",
                details = result
            });
        }
        else
        {
            return BadRequest(new
            {
                message = $"Bulk revoke failed: {string.Join(", ", result.ErrorMessages)}",
                details = result
            });
        }
    }

    /// <summary>
    /// Initiate Google OAuth login
    /// </summary>
    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin(string returnUrl = "/")
    {
        var redirectUrl = Url.Action("GoogleCallback", "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("External login info was null");
            return BadRequest("Error loading external login information");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var googleId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
        {
            _logger.LogWarning("Required claims missing from Google response");
            return BadRequest("Required information missing from Google response");
        }

        var user = await _identityService.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user
            var names = name?.Split(' ') ?? new[] { email.Split('@')[0] };
            var firstName = names.Length > 0 ? names[0] : email.Split('@')[0];
            var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "";

            var result = await _identityService.CreateUserAsync(
                email,
                email,
                Guid.NewGuid().ToString(), // Random password since it won't be used
                firstName,
                lastName);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user from Google login: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest("Failed to create user account");
            }

            user = await _identityService.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Failed to retrieve created user");
            }

            // Set Google-specific properties
            user.GoogleId = googleId;
            user.GoogleEmail = email;
            user.IsGoogleAccount = true;
            user.GoogleLinkedAt = DateTime.UtcNow;
            user.EmailConfirmed = true; // Auto-confirm since Google verified it

            await _identityService.UpdateUserAsync(user);
        }
        else
        {
            // Update existing user with Google info if not already linked
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = googleId;
                user.GoogleEmail = email;
                user.IsGoogleAccount = true;
                user.GoogleLinkedAt = DateTime.UtcNow;
                await _identityService.UpdateUserAsync(user);
            }
        }

        // Sign in the user
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Generate JWT token
        var roles = await _identityService.GetUserRolesAsync(user);
        var token = _identityService.GenerateToken(user.UserName!, user.Email!, user.Id);

        _logger.LogInformation("User {Email} successfully logged in via Google", email);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList(),
            Message = "Login successful"
        });
    }

    /// <summary>
    /// Verify Google token and check if user exists in database
    /// </summary>
    [HttpPost("verify-google-token")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyGoogleTokenResponse>> VerifyGoogleToken([FromBody] VerifyGoogleTokenRequest request)
    {
        var command = new VerifyGoogleTokenCommand(request.GoogleToken);
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}

public record GrantPermissionRequest(string RoleName, string PermissionName);
public record RevokePermissionRequest(string RoleName, string PermissionName);
public record VerifyGoogleTokenRequest(string GoogleToken);
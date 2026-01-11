using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthManSys.Application.Modules.Users.UserInformation.Queries;
using AuthManSys.Application.Modules.Users.Registration.Commands;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Api.Models;
using AuthManSys.Application.Modules.Users.UpdateUser.Commands;
using AuthManSys.Application.Modules.Users.SoftDeleteUser.Commands;
using AuthManSys.Application.Modules.Auth.PasswordManagement.Commands;
using AuthManSys.Domain.Entities;
using System.Security.Claims;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IActivityLogRepository _activityLogRepository;

    public UserController(IMediator mediator, IActivityLogRepository activityLogService)
    {
        _mediator = mediator;
        _activityLogRepository = activityLogService;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserInformationResponse>> GetUserInformation(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserInformationQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserInformationResponse>> GetCurrentUserInformation(
        CancellationToken cancellationToken = default)
    {
        // Get username from JWT token claims
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("username")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Unable to determine username from token." });
        }

        var query = new GetUserInformationByUsernameQuery(username);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "ViewUsers")]
    public async Task<ActionResult<PagedResponse<UserInformationResponse>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] string sortBy = "Id",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var request = new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = Math.Min(pageSize, 100), // Limit page size to prevent abuse
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var query = new GetAllUsersQuery(request);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> RegisterUser(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("update-information")]
    public async Task<ActionResult<UpdateUserInformationResponse>> UpdateUserInformation(
        [FromBody] UpdateUserInformationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserInformationCommand(
            request.Username,
            request.FirstName,
            request.LastName,
            request.Email);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsUpdated)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Soft delete a user
    /// </summary>
    [HttpDelete("soft-delete")]
    [Authorize(Policy = "DeleteUsers")]
    public async Task<ActionResult<SoftDeleteUserResponse>> SoftDeleteUser(
        [FromBody] SoftDeleteUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";
        var command = new SoftDeleteUserCommand(request.Username, currentUser);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsDeleted)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Patch update user information (partial update)
    /// </summary>
    [HttpPatch("patch-information")]
    public async Task<ActionResult<UpdateUserInformationResponse>> PatchUserInformation(
        [FromBody] PatchUserInformationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Determine which fields were provided in the request
        var command = new PatchUserInformationCommand(
            request.Username,
            request.FirstName,
            request.LastName,
            request.Email,
            !string.IsNullOrEmpty(request.FirstName),
            !string.IsNullOrEmpty(request.LastName),
            !string.IsNullOrEmpty(request.Email)
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsUpdated)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get current user's username from JWT token claims
        var username = User.Identity?.Name;

        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Unable to determine user from token." });
        }

        var command = new ChangePasswordCommand(
            username,
            request.CurrentPassword,
            request.NewPassword);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsChanged)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ResetPasswordCommand(
            request.Email,
            request.Token,
            request.NewPassword);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsReset)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get last N activities for a specific user
    /// </summary>
    [HttpGet("{userId:int}/activities/last/{count:int}")]
    public async Task<ActionResult<IEnumerable<UserActivityLog>>> GetLastNUserActivities(
        int userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        // Validate parameters
        if (count <= 0 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100." });
        }

        var activities = await _activityLogRepository.GetLastNUserActivitiesAsync(
            userId,
            count,
            cancellationToken);

        return Ok(activities);
    }
}
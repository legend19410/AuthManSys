using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthManSys.Application.UserInformation.Queries;
using AuthManSys.Application.UserRegistration.Commands;
using AuthManSys.Application.Common.Models;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Api.Models;
using AuthManSys.Application.UpdateUser.Commands;
using AuthManSys.Application.SoftDeleteUser.Commands;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserInformationResponse>> GetUserInformation(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserInformationQuery(userId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user information." });
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserInformationResponse>> GetCurrentUserInformation(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Unable to determine user ID from token." });
            }

            var query = new GetUserInformationQuery(userId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user information." });
        }
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
        try
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
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving users." });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> RegisterUser(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while registering the user." });
        }
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("update-information")]
    public async Task<ActionResult<UpdateUserInformationResponse>> UpdateUserInformation(
        [FromBody] UpdateUserInformationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating user information." });
        }
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
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the user." });
        }
    }

    /// <summary>
    /// Patch update user information (partial update)
    /// </summary>
    [HttpPatch("patch-information")]
    public async Task<ActionResult<UpdateUserInformationResponse>> PatchUserInformation(
        [FromBody] PatchUserInformationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating user information." });
        }
    }
}
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthManSys.Application.UserInformation.Queries;
using AuthManSys.Application.Common.Models;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserInformationController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserInformationController(IMediator mediator)
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
}
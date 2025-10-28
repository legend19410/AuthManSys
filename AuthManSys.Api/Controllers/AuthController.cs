using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AuthManSys.Api.Models;
using AuthManSys.Application.Security.Commands.Login;
using AuthManSys.Application.Common.Models.Responses;
using AuthManSys.Application.Common.Exceptions;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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
}
using MediatR;

namespace AuthManSys.Application.Modules.Auth.GoogleAuth.Commands;

public record VerifyGoogleTokenCommand(
    string GoogleToken
) : IRequest<VerifyGoogleTokenResponse>;

public class VerifyGoogleTokenResponse
{
    public bool IsValid { get; set; }
    public bool UserExists { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? GoogleId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
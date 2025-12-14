using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class GoogleTokenRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
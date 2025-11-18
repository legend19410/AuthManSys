using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class ConfirmEmailRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
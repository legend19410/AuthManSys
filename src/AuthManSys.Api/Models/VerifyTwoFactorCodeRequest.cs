using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class VerifyTwoFactorCodeRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}
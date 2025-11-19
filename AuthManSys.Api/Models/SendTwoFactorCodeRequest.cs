using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class SendTwoFactorCodeRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Username { get; set; } = string.Empty;
}
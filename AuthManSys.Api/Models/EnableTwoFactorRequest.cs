using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class EnableTwoFactorRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
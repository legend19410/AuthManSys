using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class EnableTwoFactorRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public bool Enable { get; set; } = true;
}
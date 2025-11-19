using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class SoftDeleteUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
}
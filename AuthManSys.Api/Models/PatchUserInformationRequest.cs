using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class PatchUserInformationRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
}
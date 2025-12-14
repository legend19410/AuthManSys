using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class BulkPermissionRemovalRequest
{
    [Required]
    public List<RolePermissionMappingRequest> Permissions { get; set; } = new List<RolePermissionMappingRequest>();
}
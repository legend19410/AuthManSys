using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Api.Models;

public class BulkPermissionAssignmentRequest
{
    [Required]
    public List<RolePermissionMapping> Permissions { get; set; } = new List<RolePermissionMapping>();
}
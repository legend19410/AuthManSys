# Dynamic Permission-Based Authorization System

This document describes the complete implementation of a dynamic, runtime-configurable permission system for AuthManSys.

## 🎯 Overview

The permission system provides fine-grained, role-based access control that can be modified at runtime without code changes. It replaces static role-based authorization with a flexible, scalable permission framework.

### Why Dynamic Permissions?

| Static Roles | Dynamic Permissions |
|-------------|-------------------|
| `[Authorize(Roles = "Admin")]` | `[Authorize(Policy = "ManageUsers")]` |
| Hardcoded in attributes | Configurable at runtime |
| Role-based (coarse) | Permission-based (fine-grained) |
| Requires code changes | Admin UI configuration |
| Limited flexibility | Highly scalable |

## 🗄️ Database Schema

```sql
-- Permissions table
CREATE TABLE Permissions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Category NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- RolePermissions junction table
CREATE TABLE RolePermissions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RoleId NVARCHAR(450) NOT NULL,
    PermissionId INTEGER NOT NULL,
    GrantedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    GrantedBy NVARCHAR(450),
    UNIQUE(RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);
```

### Entity Relationships

```
ApplicationUser ←→ AspNetUserRoles ←→ AspNetRoles ←→ RolePermissions ←→ Permissions
```

## 🔧 Core Components

### 1. Permission Requirement

```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
```

### 2. Permission Authorization Handler

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var hasPermission = await _permissionService.UserHasPermissionAsync(userId, requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
```

### 3. Dynamic Policy Provider

```csharp
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (IsPermissionPolicy(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
```

## 🛠️ Service Implementation

### Permission Service Interface

```csharp
public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);
    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
    Task GrantPermissionToRoleAsync(string roleId, string permissionName, string? grantedBy = null);
    Task RevokePermissionFromRoleAsync(string roleId, string permissionName);
    Task<Dictionary<string, List<string>>> GetRolePermissionMappingsAsync();
    Task CreatePermissionAsync(string name, string? description = null, string? category = null);
}
```

### Sophisticated Caching Strategy

The system implements a multi-layered caching strategy with intelligent invalidation:

#### Cache Types
- **User Permissions**: `user_permissions_{userId}` - Cached for 30 minutes
- **Role Permissions**: `role_permissions_{roleId}` - Cached for 30 minutes
- **All Permissions**: `all_permissions` - Cached for 1 hour
- **Detailed Permissions**: `all_permissions_detailed` - Cached for 1 hour
- **Role Mappings**: `detailed_role_permission_mappings` - Cached for 30 minutes

#### Intelligent Cache Invalidation
```csharp
public interface IPermissionCacheManager
{
    Task ClearRoleCacheAsync(string roleId);
    void ClearUserCache(string userId);
    Task ClearUserCachesByRoleAsync(string roleId);
    void ClearAllPermissionCaches();
    void RegisterRoleUserRelationship(string roleId, string userId);
    void UnregisterRoleUserRelationship(string roleId, string userId);
}
```

#### Cascade Invalidation
When role permissions change, the system automatically:
1. Clears the role's permission cache
2. Finds all users with that role via database query
3. Clears each affected user's permission cache
4. Clears global role-permission mappings cache
5. Maintains thread-safe relationship tracking for performance

#### Relationship Tracking
- `ConcurrentDictionary<string, HashSet<string>> _roleToUsersMap` - Thread-safe role→users mapping
- `ConcurrentDictionary<string, HashSet<string>> _userToRolesMap` - Thread-safe user→roles mapping
- Automatic fallback to database queries if in-memory tracking is unavailable

## 🎮 Controller Usage

### Basic Permission Protection

```csharp
[HttpGet]
[Authorize(Policy = "ViewUsers")]
public async Task<IActionResult> GetUsers()
{
    // Only users with "ViewUsers" permission can access
    return Ok(await _userService.GetAllUsersAsync());
}

[HttpPost]
[Authorize(Policy = "CreateUsers")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Only users with "CreateUsers" permission can access
    return Ok(await _userService.CreateUserAsync(request));
}

[HttpDelete("{id}")]
[Authorize(Policy = "DeleteUsers")]
public async Task<IActionResult> DeleteUser(int id)
{
    // Only users with "DeleteUsers" permission can access
    return Ok(await _userService.DeleteUserAsync(id));
}
```

### Multiple Permission Levels

```csharp
// Admin-only features
[Authorize(Policy = "AccessAdminPanel")]
public IActionResult AdminDashboard() => View();

// Manager or Admin
[Authorize(Policy = "ManageUsers")]
public IActionResult UserManagement() => View();

// Read-only access
[Authorize(Policy = "ViewReports")]
public IActionResult Reports() => View();
```

## 🌱 Seeded Permissions

The system comes with comprehensive default permissions:

### User Management
- `ManageUsers` - Full user CRUD operations
- `ViewUsers` - Read user information
- `CreateUsers` - Create new users
- `EditUsers` - Modify existing users
- `DeleteUsers` - Remove users
- `ViewUserProfile` - View own profile
- `EditUserProfile` - Edit own profile

### Role Management
- `ManageRoles` - Full role management
- `ViewRoles` - View roles and assignments
- `AssignRoles` - Assign roles to users

### Permission Management
- `ManagePermissions` - Full permission management
- `ViewPermissions` - View permission assignments
- `GrantPermissions` - Grant permissions to roles
- `RevokePermissions` - Revoke permissions from roles

### System Administration
- `AccessAdminPanel` - Admin interface access
- `ViewSystemLogs` - View system logs
- `ManageSystemSettings` - System configuration

### API Access
- `AccessApiDocumentation` - API docs access
- `UsePublicApi` - Public endpoints
- `UsePrivateApi` - Private/admin endpoints

### Data Operations
- `ViewReports` - View analytics
- `ExportData` - Export functionality
- `ImportData` - Import functionality

### Security
- `ViewAuditLogs` - Security audit logs
- `ManageAuthentication` - Auth settings
- `ViewSessions` - Active sessions
- `TerminateSessions` - End user sessions

## 🔧 Registration & Configuration

### In Program.cs (or ServiceCollectionExtensions)

```csharp
// Add Authorization Services
services.AddAuthorization();
services.AddScoped<IPermissionService, PermissionService>();
services.AddScoped<IPermissionCacheManager, PermissionCacheManager>();
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
services.AddMemoryCache();

// Identity configuration
services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Identity options
}).AddEntityFrameworkStores<AuthManSysDbContext>();
```

## 🆕 Recent Enhancements (v2.0)

### Enhanced Permission Policy Provider

The `PermissionPolicyProvider` now supports additional permission patterns:

```csharp
private static bool IsPermissionPolicy(string policyName)
{
    return policyName.Contains("Manage") ||
           policyName.Contains("View") ||
           policyName.Contains("Create") ||
           policyName.Contains("Edit") ||
           policyName.Contains("Delete") ||
           policyName.Contains("Access") ||
           policyName.Contains("Grant") ||     // NEW
           policyName.Contains("Revoke");     // NEW
}
```

### Improved API Response Format

All endpoints now use consistent response format:

**Success Responses:**
```json
{"message": "Operation completed successfully"}
```

**Error Responses:**
```json
{"message": "Descriptive error message"}
```

### Role-Name Based Operations

API endpoints now accept `roleName` instead of `roleId` for better usability:

```csharp
// Old approach (roleId)
{
  "roleId": "550e8400-e29b-41d4-a716-446655440000",
  "permissionName": "ManageUsers"
}

// New approach (roleName) - More user-friendly
{
  "roleName": "Administrator",
  "permissionName": "ManageUsers"
}
```

### Enhanced Service Methods

New service methods provide better feedback:

```csharp
public interface IPermissionService
{
    // Enhanced methods that return operation status
    Task<bool> GrantPermissionToRoleByNameAsync(string roleName, string permissionName, string? grantedBy = null);
    Task<bool> RevokePermissionFromRoleByNameAsync(string roleName, string permissionName);

    // Detailed permission information
    Task<IEnumerable<PermissionDto>> GetAllPermissionsDetailedAsync();
    Task<IEnumerable<RolePermissionMappingDto>> GetDetailedRolePermissionMappingsAsync();
}
```

### Smart Response Handling

Controllers now provide specific feedback for different scenarios:

```csharp
// Grant permission responses
if (wasGranted)
    return Ok(new { message = "Permission granted successfully" });
else
    return Ok(new { message = "Permission was already assigned to this role" });

// Error responses use consistent message format
catch (InvalidOperationException ex)
{
    return BadRequest(new { message = ex.Message });
}
```

## 📊 Default Role Mappings

```csharp
Administrator: [
    "ManageUsers", "ViewUsers", "CreateUsers", "EditUsers", "DeleteUsers",
    "ManageRoles", "ViewRoles", "AssignRoles",
    "ManagePermissions", "ViewPermissions", "GrantPermissions", "RevokePermissions",
    "AccessAdminPanel", "ViewSystemLogs", "ManageSystemSettings",
    "UsePublicApi", "UsePrivateApi", "ViewReports", "ExportData", "ImportData",
    "ViewAuditLogs", "ManageAuthentication", "ViewSessions", "TerminateSessions"
]

Manager: [
    "ViewUsers", "CreateUsers", "EditUsers",
    "ViewRoles", "AssignRoles",
    "ViewPermissions",
    "UsePublicApi", "ViewReports", "ExportData",
    "ViewAuditLogs", "ViewSessions"
]

User: [
    "ViewUserProfile", "EditUserProfile",
    "UsePublicApi", "ViewReports"
]

ReadOnly: [
    "ViewUsers", "ViewRoles", "ViewPermissions",
    "UsePublicApi", "ViewReports"
]
```

## 🖥️ Admin UI Examples

### Permission Management API

```bash
# Get all permissions with detailed information
GET /api/Auth/permissions
Authorization: Bearer {token}
Policy: ViewPermissions

Response:
[
  {
    "id": 1,
    "name": "ManageUsers",
    "description": "Manage user accounts",
    "category": "User Management",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
]

# Get detailed role-permission mappings
GET /api/Auth/permissions/role-mappings
Authorization: Bearer {token}
Policy: ViewPermissions

Response:
[
  {
    "roleId": "admin-role-id",
    "roleName": "Administrator",
    "roleDescription": "System administrator",
    "permissions": [/* detailed permission objects */]
  }
]

# Grant permission to role (by role name)
POST /api/Auth/permissions/grant
Authorization: Bearer {token}
Policy: GrantPermissions
Content-Type: application/json

{
  "roleName": "Manager",
  "permissionName": "ViewReports"
}

Responses:
- Success (new): {"message": "Permission granted successfully"}
- Already exists: {"message": "Permission was already assigned to this role"}
- Role not found: {"message": "Role 'Manager' not found"} (400)
- Permission not found: {"message": "Permission 'ViewReports' not found or inactive"} (400)

# Revoke permission from role (by role name)
POST /api/Auth/permissions/revoke
Authorization: Bearer {token}
Policy: RevokePermissions
Content-Type: application/json

{
  "roleName": "Manager",
  "permissionName": "ViewReports"
}

Responses:
- Success: {"message": "Permission revoked successfully"}
- Not assigned: {"message": "Permission was not assigned to this role"}
- Role not found: {"message": "Role 'Manager' not found"} (400)

# Check if current user has specific permission
GET /api/Auth/permissions/check/{permissionName}
Authorization: Bearer {token}

Response: {"hasPermission": true}

# Get current user's permissions
GET /api/Auth/permissions/my-permissions
Authorization: Bearer {token}

Response: ["ManageUsers", "ViewReports", "ExportData"]
```

### Frontend Permission Management

```html
<!-- Role Permission Matrix -->
<table class="permission-matrix">
  <thead>
    <tr>
      <th>Permission</th>
      <th>Administrator</th>
      <th>Manager</th>
      <th>User</th>
      <th>ReadOnly</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>ManageUsers</td>
      <td><input type="checkbox" checked data-role="Administrator" data-permission="ManageUsers"></td>
      <td><input type="checkbox" data-role="Manager" data-permission="ManageUsers"></td>
      <td><input type="checkbox" disabled data-role="User" data-permission="ManageUsers"></td>
      <td><input type="checkbox" disabled data-role="ReadOnly" data-permission="ManageUsers"></td>
    </tr>
    <!-- More permissions... -->
  </tbody>
</table>
```

## 🚀 Usage Examples

### Console Commands

```bash
# Database operations
dotnet run -- db status
dotnet run -- db seed     # Includes permission seeding
dotnet run -- db reset    # Resets with permissions

# Check database for permission tables
DESCRIBE Permissions;
DESCRIBE RolePermissions;
```

### Testing Permissions

```bash
# Test endpoints with different users
curl -H "Authorization: Bearer <admin-token>" http://localhost:8081/api/admin/dashboard
curl -H "Authorization: Bearer <user-token>" http://localhost:8081/api/permission/my-permissions
curl -H "Authorization: Bearer <manager-token>" http://localhost:8081/api/admin/reports
```

## 🔒 Security Features

1. **Principle of Least Privilege**: Users only get permissions they need
2. **Cache Security**: Cached permissions auto-expire and invalidate on changes
3. **Audit Trail**: All permission grants/revokes are logged with timestamps and granters
4. **Permission Inheritance**: Users inherit permissions from all assigned roles
5. **Dynamic Policies**: No hard-coded permissions in controllers

## 📈 Performance Optimizations

1. **Memory Caching**: Aggressive caching with smart invalidation
2. **Efficient Queries**: Optimized EF Core queries with proper joins
3. **Lazy Loading**: Permissions loaded on-demand
4. **Batch Operations**: Bulk permission assignments supported

## 🔧 Migration Commands

```bash
# Create permission tables migration
dotnet ef migrations add AddPermissionSystem --project AuthManSys.Infrastructure

# Apply migration
dotnet ef database update --project AuthManSys.Infrastructure

# Or use console app
dotnet run -- db migrate
```

## 🎯 Next Steps

1. **Web UI**: Build comprehensive admin interface for permission management
2. **API Versioning**: Version permission APIs for backward compatibility
3. **Permission Groups**: Group related permissions for easier management
4. **Time-based Permissions**: Add expiration dates to permissions
5. **Permission Conditions**: Add conditional logic (IP, time, etc.)
6. **Audit Dashboard**: Visual audit trail and permission analytics

This permission system provides a robust foundation for enterprise-level authorization that scales with your application needs while maintaining security and performance.
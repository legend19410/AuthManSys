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

### Caching Strategy

The service implements memory caching for performance:

- **User Permissions**: Cached for 30 minutes
- **Role Permissions**: Cached for 30 minutes
- **All Permissions**: Cached for 1 hour
- **Cache Invalidation**: Automatic on permission changes

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
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
services.AddMemoryCache();

// Identity configuration
services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Identity options
}).AddEntityFrameworkStores<AuthManSysDbContext>();
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
# Get all permissions
GET /api/permission

# Get role-permission mappings
GET /api/permission/role-mappings

# Grant permission to role
POST /api/permission/grant
{
  "roleId": "role-uuid",
  "permissionName": "ManageUsers"
}

# Revoke permission from role
POST /api/permission/revoke
{
  "roleId": "role-uuid",
  "permissionName": "ManageUsers"
}

# Check user permission
GET /api/permission/check/ManageUsers

# Get current user permissions
GET /api/permission/my-permissions
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
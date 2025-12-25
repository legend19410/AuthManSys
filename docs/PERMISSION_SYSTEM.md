# Permission System Documentation

## Overview

The AuthManSys permission system provides fine-grained access control using a role-based permission model. The system is built on ASP.NET Core Identity with custom entities for permissions and role-permission mappings.

## Architecture

### Core Entities

#### Permission
- **Id**: Primary key (auto-increment)
- **Name**: Unique permission identifier (e.g., "ManageUsers", "ViewReports")
- **Description**: Human-readable description of the permission
- **Category**: Logical grouping of permissions (e.g., "User Management", "System Administration")
- **IsActive**: Soft delete flag for permissions
- **CreatedAt**: Timestamp of permission creation
- **UpdatedAt**: Timestamp of last permission update

#### RolePermission
- **Id**: Primary key (auto-increment)
- **RoleId**: Foreign key to ASP.NET Identity Role
- **PermissionId**: Foreign key to Permission entity
- **GrantedAt**: Timestamp when permission was granted to role
- **GrantedBy**: User who granted the permission (system or user ID)

### Permission Categories

The system organizes permissions into logical categories:

1. **User Management**
   - `ManageUsers` - Create, update, and delete users
   - `ViewUsers` - View user information and lists
   - `CreateUsers` - Create new users
   - `EditUsers` - Modify existing users
   - `DeleteUsers` - Delete users
   - `ViewUserProfile` - View user profile details
   - `EditUserProfile` - Edit user profile

2. **Role Management**
   - `ManageRoles` - Create, update, and delete roles
   - `ViewRoles` - View roles and role assignments
   - `AssignRoles` - Assign roles to users

3. **Permission Management**
   - `ManagePermissions` - Manage permission assignments
   - `ViewPermissions` - View permission assignments
   - `GrantPermissions` - Grant permissions to roles
   - `RevokePermissions` - Revoke permissions from roles

4. **System Administration**
   - `AccessAdminPanel` - Access administrative interface
   - `ViewSystemLogs` - View system logs and audit trails
   - `ManageSystemSettings` - Modify system configuration

5. **API Access**
   - `AccessApiDocumentation` - Access API documentation
   - `UsePublicApi` - Use public API endpoints
   - `UsePrivateApi` - Use private/admin API endpoints

6. **Data Access**
   - `ViewReports` - View reports and analytics
   - `ExportData` - Export data to files
   - `ImportData` - Import data from files

7. **Security**
   - `ViewAuditLogs` - View audit logs and security events
   - `ManageAuthentication` - Manage authentication settings
   - `ViewSessions` - View active user sessions
   - `TerminateSessions` - Terminate user sessions

## Default Role Assignments

### Administrator Role
Has ALL permissions for complete system access:
- All User Management permissions
- All Role Management permissions
- All Permission Management permissions
- All System Administration permissions
- All API Access permissions
- All Data Access permissions
- All Security permissions

### Manager Role
Management-level access with operational permissions:
- `ViewUsers`, `CreateUsers`, `EditUsers`
- `ViewRoles`, `AssignRoles`
- `ViewPermissions`
- `AccessApiDocumentation`, `UsePublicApi`
- `ViewReports`, `ExportData`
- `ViewAuditLogs`, `ViewSessions`

### User Role
Standard user access for daily operations:
- `ViewUserProfile`, `EditUserProfile`
- `UsePublicApi`
- `ViewReports`

### ReadOnly Role
View-only access with no modification permissions:
- `ViewUsers`, `ViewRoles`, `ViewPermissions`
- `UsePublicApi`, `ViewReports`

## Implementation Details

### Database Schema

```sql
-- Permissions table
CREATE TABLE Permissions (
    Id int PRIMARY KEY AUTO_INCREMENT,
    Name varchar(100) NOT NULL UNIQUE,
    Description varchar(500),
    Category varchar(100),
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime NOT NULL,
    UpdatedAt datetime NOT NULL
);

-- Role-Permission mapping table
CREATE TABLE RolePermissions (
    Id int PRIMARY KEY AUTO_INCREMENT,
    RoleId varchar(255) NOT NULL,
    PermissionId int NOT NULL,
    GrantedAt datetime NOT NULL,
    GrantedBy varchar(255),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id),
    UNIQUE KEY UK_RolePermissions_RoleId_PermissionId (RoleId, PermissionId)
);
```

### Permission Checking

The system provides authorization through:

1. **PermissionAuthorizationHandler**: Custom authorization handler that checks user permissions
2. **PermissionRepository**: Data access layer for permission queries
3. **PermissionCacheManager**: Caching layer for performance optimization

#### Usage in Controllers

```csharp
[Authorize(Policy = "ManageUsers")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Implementation
}
```

#### Custom Permission Checks

```csharp
public async Task<bool> UserHasPermission(string userId, string permission)
{
    var userPermissions = await _permissionRepository.GetUserPermissionsAsync(userId);
    return userPermissions.Any(p => p.Name == permission && p.IsActive);
}
```

### Seeding System

The permission system is initialized through modular seeders:

1. **PermissionSeeder**: Creates all permission definitions
2. **RoleSeeder**: Creates default roles
3. **RolePermissionSeeder**: Assigns permissions to roles
4. **UserRoleSeeder**: Assigns roles to users
5. **MasterSeeder**: Orchestrates all seeders in correct order

#### Seeding Commands

```bash
# Seed everything
dotnet run --project src/AuthManSys.Console -- db seed

# Reset and reseed
dotnet run --project src/AuthManSys.Console -- db reset
```

## Security Considerations

### Best Practices

1. **Principle of Least Privilege**: Users should only have the minimum permissions needed for their role
2. **Regular Auditing**: Review permission assignments regularly
3. **Permission Granularity**: Use specific permissions rather than broad access rights
4. **Soft Delete**: Permissions are soft-deleted (IsActive flag) to maintain audit trails
5. **Audit Logging**: All permission grants/revokes are logged with timestamps and actors

### Caching Strategy

- **User Permissions**: Cached per user session for performance
- **Role Mappings**: Cached and invalidated when role assignments change
- **Permission Definitions**: Cached as they rarely change
- **Cache Invalidation**: Automatic when permissions or role assignments are modified

## API Endpoints

### Permission Management

```http
GET /api/permissions - Get all permissions (requires ViewPermissions)
GET /api/permissions/user/{userId} - Get user permissions (requires ViewPermissions)
POST /api/permissions/grant - Grant permission to role (requires GrantPermissions)
DELETE /api/permissions/revoke - Revoke permission from role (requires RevokePermissions)
```

### Role Management

```http
GET /api/roles - Get all roles (requires ViewRoles)
POST /api/roles/assign - Assign role to user (requires AssignRoles)
DELETE /api/roles/remove - Remove role from user (requires AssignRoles)
```

## Troubleshooting

### Common Issues

1. **Permission Not Working**: Check if user has the role and role has the permission
2. **Cache Issues**: Clear permission cache or restart application
3. **Seeding Problems**: Ensure database is properly migrated before seeding

### Debugging Commands

```bash
# Check database status
dotnet run --project src/AuthManSys.Console -- db status

# List all permissions
dotnet run --project src/AuthManSys.Console -- permissions list

# Check user permissions
dotnet run --project src/AuthManSys.Console -- user permissions <username>
```

## Future Enhancements

1. **Dynamic Permissions**: Runtime permission creation through API
2. **Permission Groups**: Hierarchical permission organization
3. **Time-based Permissions**: Temporary permission grants
4. **Resource-based Permissions**: Object-level permission control
5. **Permission Templates**: Predefined permission sets for rapid role creation
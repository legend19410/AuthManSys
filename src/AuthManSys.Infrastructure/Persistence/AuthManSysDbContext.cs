using Microsoft.EntityFrameworkCore;
using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Infrastructure.Persistence;

public class AuthManSysDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>, IAuthManSysDbContext
{
    public AuthManSysDbContext(DbContextOptions<AuthManSysDbContext> options) : base(options)
    {
    }

    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public async Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Get user roles from Identity framework using the proper DbSets
        var userRoleQuery = from ur in Set<ApplicationUserRole>()
                           join r in Set<IdentityRole>() on ur.RoleId equals r.Id
                           where ur.UserId == user.Id
                           select new {
                               RoleId = r.Id,
                               RoleName = r.Name,
                               RoleDescription = EF.Property<string>(r, "Description"),
                               AssignedAt = ur.AssignedAt
                           };

        var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

        var userRoles = userRoleData.Select(rd => new UserRoleDto(
            rd.RoleId, // Use original string role ID
            rd.RoleName ?? "Unknown",
            rd.RoleDescription ?? rd.RoleName ?? "Unknown", // Using role name as description temporarily
            rd.AssignedAt // Role assignment date - now tracked in AspNetUserRoles
        )).ToList();

        // Use LastPasswordChangedDate as a proxy for creation date, or a reasonable default
        DateTime createdAt = user.LastPasswordChangedDate != default(DateTime)
            ? user.LastPasswordChangedDate
            : DateTime.UtcNow.AddDays(-30); // Default fallback if not set

        // IsActive: user is active if not locked out
        bool isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new UserInformationResponse(
            user.UserId,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            isActive,
            createdAt,
            user.LastLoginAt, // LastLoginAt - now tracked in ApplicationUser
            user.IsTwoFactorEnabled,
            userRoles.AsReadOnly()
        );
    }

    public async Task<UserInformationResponse?> GetUserInformationByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Get user roles from Identity framework using the proper DbSets
        var userRoleQuery = from ur in Set<ApplicationUserRole>()
                           join r in Set<IdentityRole>() on ur.RoleId equals r.Id
                           where ur.UserId == user.Id
                           select new {
                               RoleId = r.Id,
                               RoleName = r.Name,
                               RoleDescription = EF.Property<string>(r, "Description"),
                               AssignedAt = ur.AssignedAt
                           };

        var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

        var userRoles = userRoleData.Select(rd => new UserRoleDto(
            rd.RoleId, // Use original string role ID
            rd.RoleName ?? "Unknown",
            rd.RoleDescription ?? rd.RoleName ?? "Unknown", // Using role name as description temporarily
            rd.AssignedAt // Role assignment date - now tracked in AspNetUserRoles
        )).ToList();

        // Use LastPasswordChangedDate as a proxy for creation date, or a reasonable default
        DateTime createdAt = user.LastPasswordChangedDate != default(DateTime)
            ? user.LastPasswordChangedDate
            : DateTime.UtcNow.AddDays(-30); // Default fallback if not set

        // IsActive: user is active if not locked out
        bool isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new UserInformationResponse(
            user.UserId,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            isActive,
            createdAt,
            user.LastLoginAt, // LastLoginAt - now tracked in ApplicationUser
            user.IsTwoFactorEnabled,
            userRoles.AsReadOnly()
        );
    }

    public async Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Users.Where(u => !u.IsDeleted).AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u =>
                u.UserName!.Contains(request.SearchTerm) ||
                u.Email!.Contains(request.SearchTerm) ||
                u.FirstName.Contains(request.SearchTerm) ||
                u.LastName.Contains(request.SearchTerm));
        }

        // Apply sorting
        switch (request.SortBy.ToLower())
        {
            case "username":
                query = request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName);
                break;
            case "email":
                query = request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                break;
            case "firstname":
                query = request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                break;
            case "lastname":
                query = request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                break;
            default:
                query = request.SortDescending ? query.OrderByDescending(u => u.UserId) : query.OrderBy(u => u.UserId);
                break;
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Convert to UserInformationResponse
        var userResponses = new List<UserInformationResponse>();
        foreach (var user in users)
        {
            // Get user roles with custom fields using EF.Property
            var userRoleQuery = from ur in Set<ApplicationUserRole>()
                               join r in Set<IdentityRole>() on ur.RoleId equals r.Id
                               where ur.UserId == user.Id
                               select new {
                                   RoleId = r.Id,
                                   RoleName = r.Name,
                                   RoleDescription = EF.Property<string>(r, "Description"),
                                   AssignedAt = ur.AssignedAt
                               };

            var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

            var userRoles = userRoleData.Select(rd => new UserRoleDto(
                rd.RoleId, // Use original string role ID
                rd.RoleName ?? "Unknown",
                rd.RoleDescription ?? rd.RoleName ?? "Unknown",
                rd.AssignedAt
            )).ToList();

            DateTime createdAt = user.LastPasswordChangedDate != default(DateTime)
                ? user.LastPasswordChangedDate
                : DateTime.UtcNow.AddDays(-30);

            bool isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            userResponses.Add(new UserInformationResponse(
                user.UserId,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                isActive,
                createdAt,
                user.LastLoginAt,
                user.IsTwoFactorEnabled,
                userRoles.AsReadOnly()
            ));
        }

        return new PagedResponse<UserInformationResponse>
        {
            Items = userResponses.AsReadOnly(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure custom properties on IdentityRole
        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.Property<string>("Description").HasMaxLength(500);
            entity.Property<DateTime>("CreatedAt").IsRequired();
            entity.Property<int?>("CreatedBy");
            entity.HasIndex("CreatedAt");
        });

        // Configure the custom ApplicationUserRole entity
        modelBuilder.Entity<ApplicationUserRole>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
            entity.Property(e => e.AssignedAt).IsRequired();
            entity.Property(e => e.AssignedBy);
            entity.HasIndex(e => e.AssignedAt);
            entity.HasIndex(e => new { e.UserId, e.AssignedAt });
        });

        // Configure ApplicationUser entity
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // Configure UserId as auto-increment identity column
            entity.Property(e => e.UserId)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            // Ensure UserId is unique
            entity.HasIndex(e => e.UserId)
                .IsUnique();
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
        });

        // Configure RolePermission entity
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique constraint: one permission per role
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            // Foreign key relationships
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.Property(e => e.JwtId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreationDate).IsRequired();
            entity.Property(e => e.ExpirationDate).IsRequired();
            entity.Property(e => e.Used).IsRequired();
            entity.Property(e => e.Invalidated).IsRequired();
            entity.Property(e => e.UserId).IsRequired();

            // Add index on Token for performance
            entity.HasIndex(e => e.Token).IsUnique();

            // Add index on JwtId for performance
            entity.HasIndex(e => e.JwtId);
        });

        // Configure UserActivityLog entity
        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasMaxLength(255);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.IPAddress).HasMaxLength(100);
            entity.Property(e => e.Device).HasMaxLength(200);
            entity.Property(e => e.Platform).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Metadata).HasColumnType("TEXT");

            // Add indexes for common queries
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
        });

    }
}
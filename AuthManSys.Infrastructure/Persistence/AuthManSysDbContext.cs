using Microsoft.EntityFrameworkCore;
using AuthManSys.Domain.Entities;
using AuthManSys.Application.Common.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Infrastructure.Persistence;

public class AuthManSysDbContext :  IdentityDbContext<ApplicationUser>, IAuthManSysDbContext
{
    public AuthManSysDbContext(DbContextOptions<AuthManSysDbContext> options) : base(options)
    {
    }

    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    public async Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Get user roles from Identity framework using the proper DbSets
        var userRoleQuery = from ur in Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                           join r in Set<Microsoft.AspNetCore.Identity.IdentityRole>() on ur.RoleId equals r.Id
                           where ur.UserId == user.Id
                           select new { RoleId = r.Id, RoleName = r.Name };

        var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

        var userRoles = userRoleData.Select(rd => new UserRoleDto(
            Math.Abs(rd.RoleId.GetHashCode()), // Convert GUID to positive integer in memory
            rd.RoleName ?? "Unknown",
            rd.RoleName ?? "Unknown", // Using role name as description
            DateTime.UtcNow // Role assignment date - not tracked by default Identity
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
            null, // LastLoginAt - would need custom tracking, not available in default Identity
            userRoles.AsReadOnly()
        );
    }

    public async Task<PagedResponse<UserInformationResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Users.AsQueryable();

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
            // Get user roles
            var userRoleQuery = from ur in Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                               join r in Set<Microsoft.AspNetCore.Identity.IdentityRole>() on ur.RoleId equals r.Id
                               where ur.UserId == user.Id
                               select new { RoleId = r.Id, RoleName = r.Name };

            var userRoleData = await userRoleQuery.ToListAsync(cancellationToken);

            var userRoles = userRoleData.Select(rd => new UserRoleDto(
                Math.Abs(rd.RoleId.GetHashCode()),
                rd.RoleName ?? "Unknown",
                rd.RoleName ?? "Unknown",
                DateTime.UtcNow
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
                null,
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
    }
}
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

    public async Task<UserInformationResponse?> GetUserInformationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Get user roles from Identity
        var userRoles = await UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new UserRoleDto(
                    int.Parse(r.Id),
                    r.Name!,
                    r.Name!,
                    DateTime.UtcNow
                ))
            .ToListAsync(cancellationToken);

        return new UserInformationResponse(
            user.UserId,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
            DateTime.UtcNow, // CreatedAt - Identity doesn't have this by default
            DateTime.UtcNow, // LastLoginAt - would need custom tracking
            userRoles.AsReadOnly()
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints here if needed
      
    }
}
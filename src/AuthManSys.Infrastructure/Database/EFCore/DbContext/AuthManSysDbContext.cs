using Microsoft.EntityFrameworkCore;
using AuthManSys.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthManSys.Infrastructure.Database.Entities;

namespace AuthManSys.Infrastructure.Database.EFCore.DbContext;

public class AuthManSysDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public AuthManSysDbContext(DbContextOptions<AuthManSysDbContext> options) : base(options)
    {
    }

    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<AuthManSys.Infrastructure.Database.Entities.Permission> Permissions { get; set; }
    public DbSet<AuthManSys.Infrastructure.Database.Entities.RolePermission> RolePermissions { get; set; }
    public DbSet<AuthManSys.Infrastructure.Database.Entities.RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuthManSys.Infrastructure.Database.Entities.UserActivityLog> UserActivityLogs { get; set; }

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
            // Configure UserId - manually managed, not auto-increment
            entity.Property(e => e.UserId)
                .ValueGeneratedNever();

            // Ensure UserId is unique
            entity.HasIndex(e => e.UserId)
                .IsUnique();
        });

        // Configure Permission entity
        modelBuilder.Entity<Entities.Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
        });

        // Configure RolePermission entity
        modelBuilder.Entity<Entities.RolePermission>(entity =>
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
        modelBuilder.Entity<Entities.RefreshToken>(entity =>
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
        modelBuilder.Entity<Entities.UserActivityLog>(entity =>
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
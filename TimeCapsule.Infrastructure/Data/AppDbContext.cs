using Microsoft.EntityFrameworkCore;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Capsule> Capsules => Set<Capsule>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // UserRole composite PK
        mb.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

        // Unique constraints
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        mb.Entity<RefreshToken>().HasIndex(rt => rt.TokenHash).IsUnique();

        // Capsule indexes for scheduler performance
        mb.Entity<Capsule>()
            .HasIndex(c => new { c.DeliverAt, c.Status });
        mb.Entity<Capsule>()
            .HasIndex(c => c.SenderUserId);
        mb.Entity<Capsule>()
            .HasIndex(c => c.RecipientUserId);

        // Store enums as strings
        mb.Entity<Capsule>()
            .Property(c => c.Status)
            .HasConversion<string>();
        mb.Entity<Capsule>()
            .Property(c => c.DeliveryChannel)
            .HasConversion<string>();
    }
}

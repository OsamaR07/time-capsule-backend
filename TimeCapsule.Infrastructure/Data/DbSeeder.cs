using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "ROLE_USER" },
                new Role { Name = "ROLE_ADMIN" }
            );
            db.SaveChanges();
        }
    }
}

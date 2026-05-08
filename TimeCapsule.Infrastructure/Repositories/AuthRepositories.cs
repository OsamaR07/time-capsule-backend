using Microsoft.EntityFrameworkCore;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Infrastructure.Data;

namespace TimeCapsule.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        return await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(Guid userId, string roleName)
    {
        var role = await _db.Roles.FirstAsync(r => r.Name == roleName);
        await _db.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = role.Id });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _db.RefreshTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> FindByHashAsync(string hash)
    {
        return await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.Revoked)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revoked, true));
    }
}

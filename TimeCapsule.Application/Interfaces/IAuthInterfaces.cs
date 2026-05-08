using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<RefreshResult> RefreshAsync(string refreshToken);
}

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByIdWithRolesAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task AssignRoleAsync(Guid userId, string roleName);
    Task<IEnumerable<string>> GetRolesAsync(Guid userId);
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> FindByHashAsync(string hash);
    Task UpdateAsync(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId);
}

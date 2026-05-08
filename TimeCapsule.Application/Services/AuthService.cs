using AutoMapper;
using Microsoft.Extensions.Configuration;
using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Domain.Exceptions;

namespace TimeCapsule.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IJwtService jwtService,
        IMapper mapper,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtService = jwtService;
        _mapper = mapper;
        _config = config;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepo.ExistsByEmailAsync(request.Email))
            throw new ConflictException("email_taken", "This email is already registered.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)
        };

        await _userRepo.AddAsync(user);
        await _userRepo.AssignRoleAsync(user.Id, "ROLE_USER");

        return new RegisterResponse
        {
            Message = "Registration successful. Please verify your email.",
            User = _mapper.Map<UserSummary>(user) with { Roles = new[] { "ROLE_USER" } }
        };
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException("invalid_credentials", "Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("invalid_credentials", "Invalid email or password.");

        var roles = await _userRepo.GetRolesAsync(user.Id);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshRaw = _jwtService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashToken(refreshRaw),
            ExpiresAt = DateTime.UtcNow.AddSeconds(
                int.Parse(_config["JWT_REFRESH_TTL"] ?? "604800"))
        };

        await _refreshTokenRepo.AddAsync(refreshToken);

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshRaw,
            User = _mapper.Map<UserSummary>(user) with { Roles = roles }
        };
    }

    public async Task<RefreshResult> RefreshAsync(string refreshToken)
    {
        var hash = _jwtService.HashToken(refreshToken);
        var token = await _refreshTokenRepo.FindByHashAsync(hash);

        if (token == null || token.Revoked || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("invalid_refresh_token", "Please log in again.");

        // Rotate token
        token.Revoked = true;
        await _refreshTokenRepo.UpdateAsync(token);

        var user = await _userRepo.GetByIdAsync(token.UserId)
            ?? throw new UnauthorizedException("user_not_found", "User not found.");

        var roles = await _userRepo.GetRolesAsync(user.Id);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshRaw = _jwtService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashToken(newRefreshRaw),
            ExpiresAt = DateTime.UtcNow.AddSeconds(
                int.Parse(_config["JWT_REFRESH_TTL"] ?? "604800"))
        };

        await _refreshTokenRepo.AddAsync(newRefreshToken);

        return new RefreshResult
        {
            AccessToken = newAccessToken,
            NewRefreshToken = newRefreshRaw
        };
    }
}

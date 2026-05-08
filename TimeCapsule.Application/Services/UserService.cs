using AutoMapper;
using System.Security.Cryptography;
using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Application.DTOs.User;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Exceptions;

namespace TimeCapsule.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IJwtService jwtService,
        IEmailService emailService,
        IMapper mapper)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtService = jwtService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdWithRolesAsync(userId)
            ?? throw new NotFoundException("user_not_found", "User not found.");
        
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserSummary> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("user_not_found", "User not found.");

        if (user.Email != request.Email && await _userRepo.ExistsByEmailAsync(request.Email))
            throw new ConflictException("email_taken", "This email is already registered.");

        user.Name = request.Name;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);
        
        var roles = await _userRepo.GetRolesAsync(user.Id);
        return _mapper.Map<UserSummary>(user) with { Roles = roles };
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("user_not_found", "User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("invalid_password", "Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);
        await _refreshTokenRepo.RevokeAllForUserAsync(userId); // force re-login
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await _userRepo.FindByEmailAsync(email);
        if (user is null) return; // silent fail — anti-enumeration

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        // In a real app, store this token with expiry. For now, we'll just mock it.
        await _emailService.SendPasswordResetAsync(user.Email, user.Name, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepo.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("user_not_found", "User not found.");

        // In a real app, validate the token.
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);
        await _refreshTokenRepo.RevokeAllForUserAsync(user.Id);
    }

    public async Task RevokeRefreshTokenAsync(string rawToken)
    {
        var hash = _jwtService.HashToken(rawToken);
        var token = await _refreshTokenRepo.FindByHashAsync(hash);
        if (token != null)
        {
            token.Revoked = true;
            await _refreshTokenRepo.UpdateAsync(token);
        }
    }
}

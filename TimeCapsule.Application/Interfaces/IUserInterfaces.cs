using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Application.DTOs.User;

namespace TimeCapsule.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task<UserSummary> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task SendPasswordResetEmailAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task RevokeRefreshTokenAsync(string rawToken);
}

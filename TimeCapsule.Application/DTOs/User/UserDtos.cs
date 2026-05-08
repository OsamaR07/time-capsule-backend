namespace TimeCapsule.Application.DTOs.User;

public record UserProfileResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IEnumerable<string> Roles { get; init; } = new List<string>();
    public DateTime CreatedAt { get; init; }
}

public record UpdateProfileRequest(string Name, string Email);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

namespace TimeCapsule.Application.DTOs.Auth;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UserSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IEnumerable<string> Roles { get; init; } = new List<string>();
}

public record LoginResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserSummary User { get; set; } = null!;
}

public record RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public UserSummary User { get; set; } = null!;
}

public record RefreshResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string NewRefreshToken { get; set; } = string.Empty;
}

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

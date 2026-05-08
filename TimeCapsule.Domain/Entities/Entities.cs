using TimeCapsule.Domain.Enums;

namespace TimeCapsule.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // ROLE_USER, ROLE_ADMIN
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;  // SHA-256 of raw token
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Capsule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Sender (denormalized from JWT at creation time)
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;

    // Recipient (may be guest)
    public Guid? RecipientUserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }  // E.164 format

    // Content
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Delivery
    public DeliveryChannel DeliveryChannel { get; set; }
    public DateTime DeliverAt { get; set; }

    // Lifecycle
    public CapsuleStatus Status { get; set; } = CapsuleStatus.Scheduled;
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

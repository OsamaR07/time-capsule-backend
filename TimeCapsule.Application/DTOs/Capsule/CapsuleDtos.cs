using TimeCapsule.Domain.Enums;

namespace TimeCapsule.Application.DTOs.Capsule;

public record CreateCapsuleRequest(
    string Subject,
    string Message,
    DateTime DeliverAt,
    DeliveryChannel DeliveryChannel,
    string? RecipientEmail,
    string? RecipientPhone,
    string? RecipientName
);

public record UpdateCapsuleRequest(
    string Subject,
    string Message,
    DateTime DeliverAt,
    DeliveryChannel DeliveryChannel,
    string? RecipientEmail,
    string? RecipientPhone,
    string? RecipientName
);

public record CapsuleResponse
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime DeliverAt { get; init; }
    public DeliveryChannel DeliveryChannel { get; init; }
    public CapsuleStatus Status { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string? RecipientEmail { get; init; }
    public string? RecipientPhone { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}

public record CapsuleSummaryResponse
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public DateTime DeliverAt { get; init; }
    public DeliveryChannel DeliveryChannel { get; init; }
    public CapsuleStatus Status { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record PagedResponse<T>(
    IEnumerable<T> Content,
    int Page,
    int Size,
    long TotalElements,
    int TotalPages
);

public record CapsuleStatsResponse(
    long Total, long Scheduled, long Sent, long Cancelled, long Failed
);

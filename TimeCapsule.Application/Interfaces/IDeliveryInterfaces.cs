using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Application.Interfaces;

public interface IDeliveryService
{
    Task DeliverAsync(Capsule capsule);
}

public interface ITwilioService
{
    Task SendCapsuleAsync(Capsule capsule);
}

public interface IWppService
{
    Task SendCapsuleAsync(Capsule capsule);
}

public interface IEmailService
{
    Task SendPasswordResetAsync(string email, string name, string token);
    Task SendCapsuleAsync(Capsule capsule);
}

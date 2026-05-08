using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;
using TimeCapsule.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace TimeCapsule.Infrastructure.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IEmailService _emailService;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(IEmailService emailService, ITwilioService twilioService, ILogger<DeliveryService> logger)
    {
        _emailService = emailService;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task DeliverAsync(Capsule capsule)
    {
        Console.WriteLine($"[DEBUG] Starting Delivery for Capsule {capsule.Id}");
        Console.WriteLine($"[DEBUG] Channel: {capsule.DeliveryChannel}");

        try 
        {
            if (capsule.DeliveryChannel == DeliveryChannel.Email || capsule.DeliveryChannel == DeliveryChannel.Both)
            {
                Console.WriteLine("[DEBUG] Sending Email...");
                await _emailService.SendCapsuleAsync(capsule);
                Console.WriteLine("[DEBUG] Email sent successfully.");
            }

            if (capsule.DeliveryChannel == DeliveryChannel.WhatsApp || capsule.DeliveryChannel == DeliveryChannel.Both)
            {
                Console.WriteLine("[DEBUG] Sending WhatsApp via Twilio...");
                await _twilioService.SendCapsuleAsync(capsule);
                Console.WriteLine("[DEBUG] WhatsApp task finished.");
            }
            
            Console.WriteLine("[DEBUG] All delivery steps completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ERROR in DeliveryService: {ex.Message}");
            throw;
        }
    }
}

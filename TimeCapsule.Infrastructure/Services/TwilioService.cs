using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Services;

public class TwilioService : ITwilioService
{
    private readonly ILogger<TwilioService> _logger;
    private readonly string _sid;
    private readonly string _token;
    private readonly string _from;

    public TwilioService(IConfiguration config, ILogger<TwilioService> logger)
    {
        _logger = logger;
        _sid = config["TWILIO_ACCOUNT_SID"] ?? "";
        _token = config["TWILIO_AUTH_TOKEN"] ?? "";
        _from = config["TWILIO_WHATSAPP_FROM"] ?? "";
    }

    public async Task SendCapsuleAsync(Capsule capsule)
    {
        if (string.IsNullOrEmpty(_sid) || _sid.Contains("xxxx"))
        {
            Console.WriteLine("[DEBUG] Twilio SID is missing or invalid.");
            return;
        }

        try
        {
            var to = capsule.RecipientPhone;
            if (string.IsNullOrEmpty(to)) return;

            if (!to.StartsWith("whatsapp:")) to = $"whatsapp:{to}";
            var from = _from;
            if (!from.StartsWith("whatsapp:")) from = $"whatsapp:{from}";

            Console.WriteLine($"[DEBUG] Calling Twilio API: To={to}");

            TwilioClient.Init(_sid, _token);

            // Use Task.WhenAny to create a 5-second timeout
            var twilioTask = MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(from),
                body: $"🕰️ *{capsule.Subject}*\n\n*From:* {capsule.SenderName}\n\n{capsule.Message}"
            );

            if (await Task.WhenAny(twilioTask, Task.Delay(5000)) == twilioTask)
            {
                await twilioTask;
                Console.WriteLine("[DEBUG] Twilio API responded successfully.");
            }
            else
            {
                throw new TimeoutException("Twilio API call timed out after 5 seconds.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Twilio Exception: {ex.Message}");
            throw;
        }
    }
}

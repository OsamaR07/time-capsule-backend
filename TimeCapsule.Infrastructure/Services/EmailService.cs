using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(string email, string name, string token)
    {
        var from = _config["MAIL_FROM"];
        if (string.IsNullOrEmpty(from)) return;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Time Capsule - Reset your password";

        var appUrl = _config["APP_URL"] ?? "http://localhost:5000";
        var resetLink = $"{appUrl}/reset-password?token={token}&email={email}";

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = $"""
                <div style="font-family: sans-serif; max-width: 600px; margin: auto;">
                  <h2>🕰️ Reset your Time Capsule Password</h2>
                  <p>Hi {name},</p>
                  <p>You requested a password reset. Click the link below to set a new password:</p>
                  <p><a href="{resetLink}">{resetLink}</a></p>
                  <hr/>
                  <small>If you didn't request this, you can safely ignore this email.</small>
                </div>
                """
        };

        await SendAsync(message);
    }

    public async Task SendCapsuleAsync(Capsule capsule)
    {
        var from = _config["MAIL_FROM"];
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(capsule.RecipientEmail)) return;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(capsule.RecipientEmail));
        message.Subject = $"A Time Capsule message from {capsule.SenderName}";

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = $"""
                <div style="font-family: sans-serif; max-width: 600px; margin: auto;">
                  <h2>🕰️ A Time Capsule from {capsule.SenderName}</h2>
                  <h3>{capsule.Subject}</h3>
                  <p>{capsule.Message}</p>
                  <hr/>
                  <small>Written on {capsule.CreatedAt:MMMM dd, yyyy}</small>
                </div>
                """
        };

        await SendAsync(message);
    }

    private async Task SendAsync(MimeMessage message)
    {
        var host = _config["MAIL_HOST"];
        var portStr = _config["MAIL_PORT"];
        
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr))
        {
            _logger.LogWarning("Email sending skipped: MAIL_HOST or MAIL_PORT not configured.");
            return;
        }

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, int.Parse(portStr), SecureSocketOptions.StartTls);
            
            var username = _config["MAIL_USERNAME"];
            var password = _config["MAIL_PASSWORD"];
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && username != "your_username")
            {
                await smtp.AuthenticateAsync(username, password);
            }

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", message.To.ToString());
            throw;
        }
    }
}

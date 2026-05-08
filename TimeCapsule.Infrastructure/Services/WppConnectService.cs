using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Services;

public class WppConnectService : IWppService
{
    private readonly HttpClient _http;
    private readonly ILogger<WppConnectService> _logger;
    private readonly string _session;
    private readonly string _secretKey;

    public WppConnectService(HttpClient http, IConfiguration config, ILogger<WppConnectService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri(config["WPP_BASE_URL"] ?? "http://localhost:21465");
        _session = config["WPP_SESSION_NAME"] ?? "timecapsule_session";
        _secretKey = config["WPP_SECRET_KEY"] ?? "MySecurePassword123";
    }

    private async Task<string?> GetTokenAsync()
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"/api/{_session}/{_secretKey}/generate-token", new { });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to generate WPP token: {Status}", response.StatusCode);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<WppTokenResponse>();
            return data?.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while generating WPP token");
            return null;
        }
    }

    public async Task SendCapsuleAsync(Capsule capsule)
    {
        _logger.LogInformation("Attempting to send WhatsApp message via WPPConnect to {Phone}", capsule.RecipientPhone);

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Could not proceed with WPP message: Token is null or empty");
            return;
        }

        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            phone = capsule.RecipientPhone,
            message = $"🕰️ *{capsule.Subject}*\n\n*From:* {capsule.SenderName}\n\n{capsule.Message}"
        };

        try
        {
            var response = await _http.PostAsJsonAsync($"/api/{_session}/send-message", payload);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WPPConnect message sent successfully to {Phone}", capsule.RecipientPhone);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("WPPConnect API Error: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending WPP message");
        }
    }

    private class WppTokenResponse
    {
        public string? Token { get; set; }
        public string? Status { get; set; }
    }
}

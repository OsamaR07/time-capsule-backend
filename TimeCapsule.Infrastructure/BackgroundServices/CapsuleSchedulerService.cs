using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Enums;

namespace TimeCapsule.Infrastructure.BackgroundServices;

public class CapsuleSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<CapsuleSchedulerService> _logger;
    private readonly IHostEnvironment _env;

    public CapsuleSchedulerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<CapsuleSchedulerService> logger,
        IHostEnvironment env)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = int.Parse(_config["SCHEDULER_INTERVAL_MS"] ?? "60000");

        _logger.LogInformation("Capsule Scheduler Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueCapsulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing due capsules.");
            }

            await Task.Delay(intervalMs, stoppingToken);
        }

        _logger.LogInformation("Capsule Scheduler Service is stopping.");
    }

    private async Task ProcessDueCapsulesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var capsuleRepo = scope.ServiceProvider.GetRequiredService<ICapsuleRepository>();
        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();
        var maxRetry = int.Parse(_config["CAPSULE_MAX_RETRY_ATTEMPTS"] ?? "3");
        var batchSize = int.Parse(_config["SCHEDULER_BATCH_SIZE"] ?? "50");

        var due = await capsuleRepo.FindDueForDeliveryAsync(DateTime.UtcNow, batchSize);

        if (due.Count > 0)
        {
            _logger.LogInformation("Processing {Count} due capsules.", due.Count);
        }

        foreach (var capsule in due)
        {
            try
            {
                await deliveryService.DeliverAsync(capsule);
                capsule.Status = CapsuleStatus.Sent;
                capsule.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Delivery failed for capsule {Id}. Error: {Message}", capsule.Id, ex.Message);
                capsule.RetryCount++;
                
                // In Development, we never mark as Failed so it keeps retrying every 30s
                if (!_env.IsDevelopment() && capsule.RetryCount >= maxRetry)
                {
                    capsule.Status = CapsuleStatus.Failed;
                    capsule.FailureReason = ex.Message;
                }
            }
            finally
            {
                capsule.UpdatedAt = DateTime.UtcNow;
                await capsuleRepo.UpdateAsync(capsule);
            }
        }
    }
}

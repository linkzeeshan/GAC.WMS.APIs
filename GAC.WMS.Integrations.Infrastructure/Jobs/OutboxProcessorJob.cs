using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Quartz;
using System.Text.Json;

namespace GAC.WMS.Integrations.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class OutboxProcessorJob : IJob
    {
        private readonly ILogger<OutboxProcessorJob> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWmsApiClient _wmsClient;
        private readonly IConfiguration _configuration;
        private readonly Random _jitterRandom;

        public OutboxProcessorJob(
            ILogger<OutboxProcessorJob> logger,
            ApplicationDbContext dbContext,
            IWmsApiClient wmsClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _wmsClient = wmsClient;
            _configuration = configuration;
            _jitterRandom = new Random();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting outbox processor job at {Time}", DateTime.UtcNow);

            try
            {
                // Get configuration
                var batchSize = _configuration.GetValue<int>("Outbox:BatchSize", 10);
                var maxAttempts = _configuration.GetValue<int>("Outbox:MaxAttempts", 5);

                // Find pending messages that are due for processing
                var pendingMessages = await _dbContext.IntegrationMessages
                    .Where(m => m.Status == IntegrationMessageStatus.Pending && m.NextAttemptAt <= DateTime.UtcNow)
                    .OrderBy(m => m.NextAttemptAt)
                    .Take(batchSize)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} pending integration messages to process", pendingMessages.Count);

                // Process each message
                foreach (var message in pendingMessages)
                {
                    try
                    {
                        // Mark as processing
                        message.Status = IntegrationMessageStatus.Processing;
                        await _dbContext.SaveChangesAsync();

                        // Process message
                        await ProcessMessageAsync(message);

                        // Mark as succeeded
                        message.Status = IntegrationMessageStatus.Succeeded;
                        message.ProcessedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();

                        _logger.LogInformation("Successfully processed integration message {Id} for {Aggregate} {AggregateId}",
                            message.Id, message.Aggregate, message.AggregateId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing integration message {Id} for {Aggregate} {AggregateId}",
                            message.Id, message.Aggregate, message.AggregateId);

                        // Update message with error details
                        message.Attempts++;
                        message.LastError = ex.Message;
                        
                        // Calculate next attempt time with exponential backoff and jitter
                        if (message.Attempts >= maxAttempts)
                        {
                            message.Status = IntegrationMessageStatus.Abandoned;
                            _logger.LogWarning("Abandoned integration message {Id} after {Attempts} attempts",
                                message.Id, message.Attempts);
                        }
                        else
                        {
                            message.Status = IntegrationMessageStatus.Pending;
                            message.NextAttemptAt = CalculateNextAttemptTime(message.Attempts);
                            _logger.LogInformation("Scheduled retry for integration message {Id}, attempt {Attempts} at {NextAttempt}",
                                message.Id, message.Attempts, message.NextAttemptAt);
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("Outbox processor job completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing outbox processor job");
            }
        }

        private async Task ProcessMessageAsync(IntegrationMessage message)
        {
            switch (message.Aggregate.ToLower())
            {
                case "purchaseorder":
                    await ProcessPurchaseOrderAsync(message);
                    break;
                case "salesorder":
                    await ProcessSalesOrderAsync(message);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported aggregate type: {message.Aggregate}");
            }
        }

        private async Task ProcessPurchaseOrderAsync(IntegrationMessage message)
        {
            // In a real implementation, this would deserialize the payload and call the WMS API
            // For now, we'll just log the action
            _logger.LogInformation("Processing purchase order integration: {AggregateId}", message.AggregateId);
            
            // Simulate API call to WMS
            await _wmsClient.CreatePurchaseOrderAsync(message.Payload);
        }

        private async Task ProcessSalesOrderAsync(IntegrationMessage message)
        {
            // In a real implementation, this would deserialize the payload and call the WMS API
            // For now, we'll just log the action
            _logger.LogInformation("Processing sales order integration: {AggregateId}", message.AggregateId);
            
            // Simulate API call to WMS
            await _wmsClient.CreateSalesOrderAsync(message.Payload);
        }

        private DateTime CalculateNextAttemptTime(int attemptCount)
        {
            // Exponential backoff with jitter
            // Base delay is 30 seconds
            var baseDelaySeconds = 30;
            
            // Calculate delay with exponential backoff: 30s, 1m, 2m, 4m, 8m, etc.
            var delaySeconds = baseDelaySeconds * Math.Pow(2, attemptCount - 1);
            
            // Add jitter (±20%)
            var jitterFactor = 0.8 + (_jitterRandom.NextDouble() * 0.4); // 0.8 to 1.2
            delaySeconds *= jitterFactor;
            
            // Cap at 1 hour
            delaySeconds = Math.Min(delaySeconds, 3600);
            
            return DateTime.UtcNow.AddSeconds(delaySeconds);
        }
    }
}

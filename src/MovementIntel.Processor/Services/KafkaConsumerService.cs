using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MovementIntel.Common.Configuration;
using MovementIntel.Processor.DTOs;
using MovementIntel.Processor.Services.Ingestion;

namespace MovementIntel.Processor.Services;

public class KafkaConsumerService(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaConfiguration> config,
    ILogger<KafkaConsumerService> logger)
    : BackgroundService {
    private readonly KafkaConfiguration _config = config.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (!_config.EnableConsumer) {
            logger.LogInformation("Kafka consumer is disabled");
            return;
        }

        await Task.Yield();

        using var consumer = BuildConsumer();

        try {
            while (!stoppingToken.IsCancellationRequested) {
                await ProcessBatchAsync(consumer, stoppingToken);
            }
        } finally {
            consumer.Close();
            logger.LogInformation("Kafka consumer stopped");
        }
    }

    private IConsumer<string, string> BuildConsumer() {
        var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AllowAutoCreateTopics = true,
        }).Build();

        consumer.Subscribe(_config.Topic);
        logger.LogInformation("Kafka consumer started - topic={Topic}, group={GroupId}",
            _config.Topic, _config.GroupId);

        return consumer;
    }

    private async Task ProcessBatchAsync(IConsumer<string, string> consumer, CancellationToken stoppingToken) {
        var batch = ConsumeBatch(consumer, stoppingToken);
        if (batch.Count == 0) {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<IEventIngestionService>();

        var accepted = await ingestionService.IngestAsync(batch, stoppingToken);

        logger.LogInformation("Kafka batch processed - size={BatchSize}, accepted={Accepted}",
            batch.Count, accepted);

        consumer.Commit();
    }

    private List<MovementEventRequest> ConsumeBatch(
        IConsumer<string, string> consumer, CancellationToken cancellationToken) {
        var batch = new List<MovementEventRequest>(_config.MaxBatchSize);
        var timeout = TimeSpan.FromMilliseconds(_config.PollTimeoutMs);

        while (batch.Count < _config.MaxBatchSize && !cancellationToken.IsCancellationRequested) {
            ConsumeResult<string, string>? result;
            try {
                result = consumer.Consume(timeout);
            } catch (ConsumeException ex) {
                logger.LogWarning(ex, "Consume error: {Reason}", ex.Error.Reason);
                break;
            } catch (OperationCanceledException) {
                break;
            }

            if (result == null) {
                break;
            }

            DeserializeMessage(result, batch);
            consumer.StoreOffset(result);
        }

        return batch;
    }

    private void DeserializeMessage(ConsumeResult<string, string> result, List<MovementEventRequest> batch) {
        try {
            var evt = JsonSerializer.Deserialize<MovementEventRequest>(result.Message.Value);
            if (evt != null) {
                batch.Add(evt);
            }
        } catch (JsonException ex) {
            logger.LogWarning(ex, "Skipping invalid JSON at offset {Offset}", result.Offset);
        }
    }
}

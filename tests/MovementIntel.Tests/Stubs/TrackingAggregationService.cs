using MovementIntel.Processor.Services.Aggregation;

namespace MovementIntel.Tests.Stubs;

public class TrackingAggregationService : IAggregationService {
    public List<(string EntityType, string EntityId, double? SpeedKmh)> Upserts { get; } = [];

    public Task UpsertAsync(
        string entityType, string entityId,
        DateTime timestamp, double? speedKmh,
        CancellationToken cancellationToken) {
        Upserts.Add((entityType, entityId, speedKmh));
        return Task.CompletedTask;
    }
}

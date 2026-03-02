using MovementIntel.Processor.Services.Aggregation;

namespace MovementIntel.Tests.Stubs;

public class StubAggregationService : IAggregationService {
    public Task UpsertAsync(
        string entityType,
        string entityId,
        DateTime timestamp,
        double? speedKmh,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

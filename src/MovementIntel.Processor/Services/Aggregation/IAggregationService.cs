namespace MovementIntel.Processor.Services.Aggregation;

public interface IAggregationService {
    Task UpsertAsync(
        string entityType,
        string entityId,
        DateTime timestamp,
        double? speedKmh,
        CancellationToken cancellationToken);
}

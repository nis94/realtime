using Microsoft.Extensions.Logging.Abstractions;
using MovementIntel.Processor.DTOs;
using MovementIntel.Processor.Services.Aggregation;
using MovementIntel.Processor.Services.Ingestion;
using MovementIntel.Processor.Services.Position;
using MovementIntel.Processor.Services.Validation;

namespace MovementIntel.Tests.Stubs;

public class StubIngestionService(
    HashSet<Guid> insertedIds,
    IEventValidator validator,
    IPositionService positionService,
    IAggregationService aggregationService)
    : EventIngestionService(null!, validator, positionService, aggregationService,
        NullLogger<EventIngestionService>.Instance) {
    protected override Task<HashSet<Guid>> InsertRawEventsAsync(
        List<(MovementEventRequest Request, Guid EventId, DateTime Timestamp)> events,
        CancellationToken cancellationToken) =>
        Task.FromResult(insertedIds);
}

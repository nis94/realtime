using MovementIntel.Processor.DTOs;
using MovementIntel.Processor.Services.Validation;
using MovementIntel.Tests.Stubs;
using Xunit;
using static MovementIntel.Tests.TestData;

namespace MovementIntel.Tests.Processor;

public class EventIngestionServiceTests {
    [Fact]
    public async Task IngestAsync_MixedBatch_OnlyValidEventsAccepted() {
        var validId = Guid.NewGuid();
        var service = new StubIngestionService(
            insertedIds: [validId],
            new EventValidator(),
            new StubPositionService(),
            new StubAggregationService());

        var events = new List<MovementEventRequest> {
            ValidEventRequest(validId.ToString()),
            new() { EventId = null }, // invalid - missing event_id
        };

        var accepted = await service.IngestAsync(events, CancellationToken.None);

        Assert.Equal(1, accepted);
    }

    [Fact]
    public async Task IngestAsync_DuplicateEventId_OnlyOneAccepted() {
        var eventId = Guid.NewGuid();
        // Simulate DB returning only one inserted ID (duplicate rejected by ON CONFLICT DO NOTHING)
        var service = new StubIngestionService(
            insertedIds: [eventId],
            new EventValidator(),
            new TrackingPositionService(),
            new TrackingAggregationService());

        var events = new List<MovementEventRequest> {
            ValidEventRequest(eventId.ToString()),
            ValidEventRequest(eventId.ToString()), // same event_id
        };

        var accepted = await service.IngestAsync(events, CancellationToken.None);

        Assert.Equal(1, accepted);
    }

    [Fact]
    public async Task IngestAsync_DuplicateNotInInserted_DerivedDataSkipped() {
        var insertedId = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        // Only insertedId came back from the DB; duplicateId was already present
        var positionTracker = new TrackingPositionService();
        var aggregationTracker = new TrackingAggregationService();
        var service = new StubIngestionService(
            insertedIds: [insertedId],
            new EventValidator(),
            positionTracker,
            aggregationTracker);

        var events = new List<MovementEventRequest> {
            ValidEventRequest(insertedId.ToString()),
            ValidEventRequest(duplicateId.ToString()),
        };

        await service.IngestAsync(events, CancellationToken.None);

        // Only the inserted event should trigger derived data updates
        Assert.Single(positionTracker.Updates);
        Assert.Single(aggregationTracker.Upserts);
    }

    [Fact]
    public async Task IngestAsync_AllInvalid_ReturnsZero() {
        var service = new StubIngestionService(
            insertedIds: [],
            new EventValidator(),
            new StubPositionService(),
            new StubAggregationService());

        var events = new List<MovementEventRequest> {
            new() { EventId = null },
            new() { EventId = "not-a-uuid" },
        };

        var accepted = await service.IngestAsync(events, CancellationToken.None);

        Assert.Equal(0, accepted);
    }

    [Fact]
    public async Task IngestAsync_EmptyBatch_ReturnsZero() {
        var service = new StubIngestionService(
            insertedIds: [],
            new EventValidator(),
            new StubPositionService(),
            new StubAggregationService());

        var accepted = await service.IngestAsync([], CancellationToken.None);

        Assert.Equal(0, accepted);
    }
}

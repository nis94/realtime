using MovementIntel.Processor.DTOs;
using Xunit;
using MovementIntel.Processor.Services.Validation;
using static MovementIntel.Tests.TestData;

namespace MovementIntel.Tests.Processor;

public class EventValidatorTests {
    private readonly EventValidator _validator = new();

    public static TheoryData<string, Action<MovementEventRequest>, string> InvalidCases => new() {
        { "missing event_id", r => r.EventId = null, "event_id" },
        { "invalid UUID", r => r.EventId = "not-a-uuid", "UUID" },
        { "missing entity", r => r.Entity = null, "entity" },
        { "missing entity.type", r => r.Entity!.Type = null, "entity.type" },
        { "missing entity.id", r => r.Entity!.Id = null, "entity.id" },
        { "missing timestamp", r => r.Timestamp = null, "timestamp" },
        { "invalid timestamp", r => r.Timestamp = "not-a-date", "ISO 8601" },
        { "missing position", r => r.Position = null, "position" },
        { "lat too low", r => r.Position!.Lat = -91, "lat" },
        { "lat too high", r => r.Position!.Lat = 91, "lat" },
        { "lon too low", r => r.Position!.Lon = -181, "lon" },
        { "lon too high", r => r.Position!.Lon = 181, "lon" },
        { "negative speed", r => r.SpeedKmh = -1, "speed_kmh" },
    };

    public static TheoryData<string, Action<MovementEventRequest>> ValidCases => new() {
        { "baseline valid event", _ => { } },
        { "null speed (optional)", r => r.SpeedKmh = null },
        { "zero speed", r => r.SpeedKmh = 0 }, {
            "boundary lat/lon", r => {
                r.Position!.Lat = 90;
                r.Position.Lon = -180;
            }
        },
        { "past timestamp", r => r.Timestamp = "2020-01-01T00:00:00Z" },
    };

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Validate_InvalidEvent_ReturnsInvalidWithExpectedError(
        string scenario,
        Action<MovementEventRequest> mutate,
        string expectedError) {
        // Arrange
        var request = ValidEventRequest();
        mutate(request);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid, scenario);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void Validate_ValidEvent_ReturnsIsValid(
        string scenario,
        Action<MovementEventRequest> mutate) {
        // Arrange
        var request = ValidEventRequest();
        mutate(request);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid, scenario);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Validate_ValidEvent_ParsedValuesCorrectlyExtracted() {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = ValidEventRequest();
        request.EventId = eventId.ToString();
        request.Timestamp = "2025-06-15T10:30:00Z";

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(eventId, result.ParsedEventId);
        Assert.Equal(new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc), result.ParsedTimestamp);
    }
}

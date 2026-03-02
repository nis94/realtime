using System.Globalization;
using MovementIntel.Processor.DTOs;

namespace MovementIntel.Processor.Services.Validation;

public class EventValidator : IEventValidator {
    public EventValidationResult Validate(MovementEventRequest request) {
        if (string.IsNullOrWhiteSpace(request.EventId)) {
            return Fail("event_id is required");
        }

        if (!Guid.TryParse(request.EventId, out var parsedEventId)) {
            return Fail($"event_id '{request.EventId}' is not a valid UUID");
        }

        if (request.Entity is null) {
            return Fail("entity is required");
        }

        if (string.IsNullOrWhiteSpace(request.Entity.Type)) {
            return Fail("entity.type is required");
        }

        if (string.IsNullOrWhiteSpace(request.Entity.Id)) {
            return Fail("entity.id is required");
        }

        if (string.IsNullOrWhiteSpace(request.Timestamp)) {
            return Fail("timestamp is required");
        }

        if (!DateTime.TryParse(request.Timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsedTimestamp)) {
            return Fail($"timestamp '{request.Timestamp}' is not a valid ISO 8601 date");
        }

        if (request.Position is null) {
            return Fail("position is required");
        }

        if (request.Position.Lat is < -90 or > 90) {
            return Fail($"position.lat {request.Position.Lat} is out of range [-90, 90]");
        }

        if (request.Position.Lon is < -180 or > 180) {
            return Fail($"position.lon {request.Position.Lon} is out of range [-180, 180]");
        }

        if (request.SpeedKmh is < 0) {
            return Fail($"speed_kmh {request.SpeedKmh.Value} must be >= 0");
        }

        return new EventValidationResult(true, null, parsedEventId, parsedTimestamp);
    }

    private static EventValidationResult Fail(string error) => new(false, error, Guid.Empty, default);
}

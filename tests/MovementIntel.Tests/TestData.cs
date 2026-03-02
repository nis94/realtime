using MovementIntel.Processor.DTOs;

namespace MovementIntel.Tests;

public static class TestData {
    public static MovementEventRequest ValidEventRequest(string? eventId = null) => new() {
        EventId = eventId ?? Guid.NewGuid().ToString(),
        Entity = new EntityDTO { Type = "vehicle", Id = "v-1" },
        Timestamp = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
        Position = new PositionDTO { Lat = 52.52, Lon = 13.405 },
        SpeedKmh = 42.3
    };
}

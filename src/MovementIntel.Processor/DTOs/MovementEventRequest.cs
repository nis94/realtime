using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovementIntel.Processor.DTOs;

public class MovementEventRequest {
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    [JsonPropertyName("entity")]
    public EntityDTO? Entity { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("position")]
    public PositionDTO? Position { get; set; }

    [JsonPropertyName("speed_kmh")]
    public double? SpeedKmh { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("attributes")]
    public JsonElement? Attributes { get; set; }
}

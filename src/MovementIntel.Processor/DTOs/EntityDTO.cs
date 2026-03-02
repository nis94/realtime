using System.Text.Json.Serialization;

namespace MovementIntel.Processor.DTOs;

public class EntityDTO {
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

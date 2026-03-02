using System.Text.Json.Serialization;

namespace MovementIntel.Processor.DTOs;

public class PositionDTO {
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}

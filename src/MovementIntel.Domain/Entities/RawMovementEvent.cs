namespace MovementIntel.Domain.Entities;

public class RawMovementEvent {
    public Guid EventId { get; set; }
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedKmh { get; set; }
    public string? Source { get; set; }
    public string? Attributes { get; set; }
    public DateTime ReceivedAt { get; set; }
}

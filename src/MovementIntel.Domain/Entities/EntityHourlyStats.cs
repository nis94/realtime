namespace MovementIntel.Domain.Entities;

public class EntityHourlyStats {
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public DateTime BucketHour { get; set; }
    public int EventCount { get; set; }
    public double MaxSpeedKmh { get; set; }
    public double SpeedSum { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace MovementIntel.Processor.Services.Position;

public record LastKnownPosition(
    double Latitude,
    double Longitude,
    double? SpeedKmh,
    DateTime Timestamp);

public interface IPositionService {
    Task<LastKnownPosition?> GetLastKnownPositionAsync(string entityType, string entityId);
    Task UpdatePositionAsync(string entityType, string entityId, double lat, double lon, double? speedKmh, DateTime timestamp);
}

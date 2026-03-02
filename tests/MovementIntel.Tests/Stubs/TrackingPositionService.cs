using MovementIntel.Processor.Services.Position;

namespace MovementIntel.Tests.Stubs;

public class TrackingPositionService : IPositionService {
    public List<(string EntityType, string EntityId, double Lat, double Lon)> Updates { get; } = [];

    public Task<LastKnownPosition?> GetLastKnownPositionAsync(string entityType, string entityId) =>
        Task.FromResult<LastKnownPosition?>(null);

    public Task UpdatePositionAsync(
        string entityType, string entityId,
        double lat, double lon,
        double? speedKmh, DateTime timestamp) {
        Updates.Add((entityType, entityId, lat, lon));
        return Task.CompletedTask;
    }
}

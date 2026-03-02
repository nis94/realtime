using MovementIntel.Processor.Services.Position;

namespace MovementIntel.Tests.Stubs;

public class StubPositionService : IPositionService {
    public Task<LastKnownPosition?> GetLastKnownPositionAsync(string entityType, string entityId) =>
        Task.FromResult<LastKnownPosition?>(null);

    public Task UpdatePositionAsync(
        string entityType, string entityId,
        double lat, double lon,
        double? speedKmh, DateTime timestamp) =>
        Task.CompletedTask;
}

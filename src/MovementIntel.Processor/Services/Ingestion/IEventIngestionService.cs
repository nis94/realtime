using MovementIntel.Processor.DTOs;

namespace MovementIntel.Processor.Services.Ingestion;

public interface IEventIngestionService {
    Task<int> IngestAsync(List<MovementEventRequest> events, CancellationToken cancellationToken);
}

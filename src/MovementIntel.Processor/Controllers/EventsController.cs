using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MovementIntel.Common.Configuration;
using MovementIntel.Processor.DTOs;
using MovementIntel.Processor.Services.Position;

namespace MovementIntel.Processor.Controllers;

// Convenience endpoint for interviewer testing - produces events to Kafka so they
// flow through the real pipeline (consumer → validate → store → aggregate).
// In production, the collection team produces directly to Kafka; this endpoint wouldn't exist.
[ApiController]
[Route("api/v1")]
public class EventsController(
    IProducer<string, string> producer,
    IOptions<KafkaConfiguration> kafkaConfig,
    IPositionService positionService) : ControllerBase {

    [HttpPost("events")]
    public async Task<IActionResult> Ingest(
        [FromBody] List<MovementEventRequest> events,
        CancellationToken cancellationToken) {
        var topic = kafkaConfig.Value.Topic;
        var produced = 0;

        foreach (var e in events) {
            var key = $"{e.Entity?.Type}:{e.Entity?.Id}";
            var value = JsonSerializer.Serialize(e);
            await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value }, cancellationToken);
            produced++;
        }

        return Accepted(new { produced, total = events.Count });
    }

    [HttpGet("entities/{entityType}/{entityId}/position")]
    public async Task<IActionResult> GetPosition(string entityType, string entityId) {
        var position = await positionService.GetLastKnownPositionAsync(entityType, entityId);
        if (position is null)
            return NotFound();

        return Ok(new {
            entity = new { type = entityType, id = entityId },
            position = new { lat = position.Latitude, lon = position.Longitude },
            speed_kmh = position.SpeedKmh,
            timestamp = position.Timestamp
        });
    }
}

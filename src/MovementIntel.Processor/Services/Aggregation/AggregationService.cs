using Microsoft.EntityFrameworkCore;
using MovementIntel.Domain;

namespace MovementIntel.Processor.Services.Aggregation;

public class AggregationService(MovementIntelDbContext db) : IAggregationService {
    public async Task UpsertAsync(
        string entityType, string entityId,
        DateTime timestamp, double? speedKmh,
        CancellationToken cancellationToken) {
        var bucketHour = new DateTime(
            timestamp.Year, timestamp.Month, timestamp.Day,
            timestamp.Hour, 0, 0, DateTimeKind.Utc);

        object speedParam = speedKmh.HasValue ? speedKmh.Value : DBNull.Value;

        object[] parameters = [entityType, entityId, bucketHour, speedParam, speedParam];

        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO entity_hourly_stats
                (entity_type, entity_id, bucket_hour, event_count, max_speed_kmh, speed_sum, updated_at)
            VALUES
                ({0}, {1}, {2}, 1, COALESCE({3}, 0), COALESCE({4}, 0), NOW())
            ON CONFLICT (entity_type, entity_id, bucket_hour)
            DO UPDATE SET
                event_count = entity_hourly_stats.event_count + 1,
                max_speed_kmh = GREATEST(entity_hourly_stats.max_speed_kmh, COALESCE(EXCLUDED.max_speed_kmh, 0)),
                speed_sum = entity_hourly_stats.speed_sum + COALESCE(EXCLUDED.speed_sum, 0),
                updated_at = NOW()
            """,
            parameters,
            cancellationToken);
    }
}

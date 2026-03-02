using Microsoft.EntityFrameworkCore;
using MovementIntel.Processor.DTOs;
using MovementIntel.Processor.Services.Aggregation;
using MovementIntel.Processor.Services.Position;
using MovementIntel.Processor.Services.Validation;
using MovementIntel.Domain;
using Npgsql;
using NpgsqlTypes;

namespace MovementIntel.Processor.Services.Ingestion;

public class EventIngestionService(
    MovementIntelDbContext db,
    IEventValidator validator,
    IPositionService positionService,
    IAggregationService aggregationService,
    ILogger<EventIngestionService> logger) : IEventIngestionService {
    public async Task<int> IngestAsync(
        List<MovementEventRequest> events,
        CancellationToken cancellationToken) {
        var validEvents = FilterValidEvents(events);
        if (validEvents.Count == 0) {
            return 0;
        }

        logger.LogDebug("Batch filtered - {Valid}/{Total} events passed validation", validEvents.Count, events.Count);

        var insertedIds = await InsertRawEventsAsync(validEvents, cancellationToken);
        await UpdateDerivedDataAsync(validEvents, insertedIds, cancellationToken);
        return insertedIds.Count;
    }

    private List<(MovementEventRequest Request, Guid EventId, DateTime Timestamp)> FilterValidEvents(
        List<MovementEventRequest> events) {
        var valid = new List<(MovementEventRequest, Guid, DateTime)>();

        foreach (var request in events) {
            var result = validator.Validate(request);
            if (!result.IsValid) {
                logger.LogWarning("Event rejected - event_id={EventId}, error={Error}", request.EventId ?? "(null)",
                    result.Error);
                continue;
            }

            valid.Add((request, result.ParsedEventId, result.ParsedTimestamp));
        }

        return valid;
    }

    private async Task UpdateDerivedDataAsync(
        List<(MovementEventRequest Request, Guid EventId, DateTime Timestamp)> events,
        HashSet<Guid> insertedIds,
        CancellationToken cancellationToken) {
        foreach (var (request, eventId, timestamp) in events) {
            if (!insertedIds.Contains(eventId)) {
                continue;
            }

            try {
                await positionService.UpdatePositionAsync(
                    request.Entity!.Type!, request.Entity.Id!,
                    request.Position!.Lat, request.Position.Lon,
                    request.SpeedKmh, timestamp);

                await aggregationService.UpsertAsync(
                    request.Entity.Type!, request.Entity.Id!,
                    timestamp, request.SpeedKmh, cancellationToken);
            } catch (Exception ex) {
                logger.LogWarning(ex, "Failed to update derived data for event {EventId}", eventId);
            }
        }
    }

    protected virtual async Task<HashSet<Guid>> InsertRawEventsAsync(
        List<(MovementEventRequest Request, Guid EventId, DateTime Timestamp)> events,
        CancellationToken cancellationToken) {
        var parameters = new List<NpgsqlParameter>();
        var valueClauses = new List<string>();

        for (var i = 0; i < events.Count; i++) {
            var (req, eventId, ts) = events[i];
            var p = $"p{i}";
            valueClauses.Add($"(@{p}_0,@{p}_1,@{p}_2,@{p}_3,@{p}_4,@{p}_5,@{p}_6,@{p}_7,@{p}_8)");

            void Add(string n, object val) => parameters.Add(new NpgsqlParameter($"{p}_{n}", val));

            void AddNullable(string n, NpgsqlDbType t, object? val) =>
                parameters.Add(new NpgsqlParameter($"{p}_{n}", t) { Value = val ?? DBNull.Value });

            Add("0", eventId);
            Add("1", req.Entity!.Type!);
            Add("2", req.Entity.Id!);
            AddNullable("3", NpgsqlDbType.TimestampTz, ts);
            Add("4", req.Position!.Lat);
            Add("5", req.Position.Lon);
            AddNullable("6", NpgsqlDbType.Double, req.SpeedKmh);
            AddNullable("7", NpgsqlDbType.Varchar, req.Source);
            AddNullable("8", NpgsqlDbType.Jsonb, req.Attributes?.GetRawText());
        }

        var sql = $"""
                   INSERT INTO raw_movement_events
                       (event_id, entity_type, entity_id, timestamp, latitude, longitude, speed_kmh, source, attributes)
                   VALUES {string.Join(",", valueClauses)}
                   ON CONFLICT (event_id) DO NOTHING
                   RETURNING event_id
                   """;

        var insertedIds = new HashSet<Guid>();
        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) {
            await conn.OpenAsync(cancellationToken);
        }

        try {
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) {
                insertedIds.Add(reader.GetGuid(0));
            }
        } catch (Exception ex) {
            logger.LogError(ex, "Batch INSERT failed for {Count} events", events.Count);
            throw;
        } finally {
            if (!wasOpen) {
                await conn.CloseAsync();
            }
        }

        return insertedIds;
    }
}

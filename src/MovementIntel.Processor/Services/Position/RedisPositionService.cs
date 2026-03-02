using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovementIntel.Common.Configuration;
using MovementIntel.Common.Constants;
using StackExchange.Redis;

namespace MovementIntel.Processor.Services.Position;

public class RedisPositionService(
    IDatabase redis,
    IOptions<IngestionConfiguration> config,
    ILogger<RedisPositionService> logger) : IPositionService {

    private readonly TimeSpan _ttl = TimeSpan.FromHours(config.Value.PositionTtlHours);

    // Lua script: atomic conditional update - only overwrite if incoming timestamp > stored timestamp
    private const string UpdateScript = """
        local key = KEYS[1]
        local newTs = tonumber(ARGV[1])
        local currentTs = tonumber(redis.call('HGET', key, 'ts') or '0')
        if newTs > currentTs then
            redis.call('HSET', key, 'lat', ARGV[2], 'lon', ARGV[3], 'speed', ARGV[4], 'ts', ARGV[1])
            redis.call('EXPIRE', key, ARGV[5])
            return 1
        end
        return 0
        """;

    public async Task<LastKnownPosition?> GetLastKnownPositionAsync(string entityType, string entityId) {
        try {
            var key = RedisKeyConstants.PositionKey(entityType, entityId);
            var entries = await redis.HashGetAllAsync(key);

            if (entries.Length == 0) {
                return null;
            }

            var dict = entries.ToDictionary(
                e => e.Name.ToString(),
                e => e.Value.ToString());

            return new LastKnownPosition(
                double.Parse(dict[RedisKeyConstants.LatField], CultureInfo.InvariantCulture),
                double.Parse(dict[RedisKeyConstants.LonField], CultureInfo.InvariantCulture),
                dict.TryGetValue(RedisKeyConstants.SpeedField, out var speed) && speed != ""
                    ? double.Parse(speed, CultureInfo.InvariantCulture)
                    : null,
                DateTime.UnixEpoch.AddMilliseconds(
                    long.Parse(dict[RedisKeyConstants.TimestampField], CultureInfo.InvariantCulture)));
        } catch (Exception ex) {
            logger.LogWarning(ex, "Failed to get position from Redis for {EntityType}:{EntityId}", entityType, entityId);
            return null;
        }
    }

    public async Task UpdatePositionAsync(
        string entityType, string entityId,
        double lat, double lon, double? speedKmh, DateTime timestamp) {
        try {
            var key = RedisKeyConstants.PositionKey(entityType, entityId);
            var tsMs = (long)(timestamp - DateTime.UnixEpoch).TotalMilliseconds;
            var ttlSeconds = (int)_ttl.TotalSeconds;

            await redis.ScriptEvaluateAsync(
                UpdateScript,
                [key],
                [
                    tsMs,
                    lat.ToString(CultureInfo.InvariantCulture),
                    lon.ToString(CultureInfo.InvariantCulture),
                    speedKmh?.ToString(CultureInfo.InvariantCulture) ?? "",
                    ttlSeconds
                ]);
        } catch (Exception ex) {
            logger.LogWarning(ex, "Failed to update position in Redis for {EntityType}:{EntityId}", entityType, entityId);
        }
    }
}

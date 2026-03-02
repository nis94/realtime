namespace MovementIntel.Common.Constants;

public static class RedisKeyConstants {
    public static string PositionKey(string entityType, string entityId) => $"position:{entityType}:{entityId}";

    public const string LatField = "lat";
    public const string LonField = "lon";
    public const string SpeedField = "speed";
    public const string TimestampField = "ts";
}

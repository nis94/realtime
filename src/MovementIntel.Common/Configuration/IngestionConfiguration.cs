namespace MovementIntel.Common.Configuration;

public class IngestionConfiguration {
    public const string SectionName = "Ingestion";

    public int PositionTtlHours { get; set; } = 72;
}

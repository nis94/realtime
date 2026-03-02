namespace MovementIntel.Common.Configuration;

public class KafkaConfiguration {
    public const string SectionName = "Kafka";

    public bool EnableConsumer { get; set; }
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "movement-events";
    public string GroupId { get; set; } = "movement-processor";
    public int MaxBatchSize { get; set; } = 500;
    public int PollTimeoutMs { get; set; } = 100;
}

using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using MovementIntel.Common.Configuration;
using MovementIntel.Domain;
using MovementIntel.Processor.Services;
using MovementIntel.Processor.Services.Aggregation;
using MovementIntel.Processor.Services.Ingestion;
using MovementIntel.Processor.Services.Position;
using MovementIntel.Processor.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<IngestionConfiguration>(
    builder.Configuration.GetSection(IngestionConfiguration.SectionName));
builder.Services.Configure<KafkaConfiguration>(
    builder.Configuration.GetSection(KafkaConfiguration.SectionName));

// Database
builder.Services.AddDbContext<MovementIntelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddSingleton(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

// Services - Singleton
builder.Services.AddSingleton<IEventValidator, EventValidator>();
builder.Services.AddSingleton<IPositionService, RedisPositionService>();

// Services - Scoped (need DbContext)
builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();

// Kafka
var kafkaConfig = builder.Configuration
    .GetSection(KafkaConfiguration.SectionName)
    .Get<KafkaConfiguration>() ?? new KafkaConfiguration();

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
    new ProducerBuilder<string, string>(new ProducerConfig {
        BootstrapServers = kafkaConfig.BootstrapServers,
    }).Build());
builder.Services.AddHostedService<KafkaConsumerService>();

// API
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<MovementIntelDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

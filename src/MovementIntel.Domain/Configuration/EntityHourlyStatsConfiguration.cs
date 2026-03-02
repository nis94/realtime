using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovementIntel.Domain.Entities;

namespace MovementIntel.Domain.Configuration;

public class EntityHourlyStatsConfiguration : IEntityTypeConfiguration<EntityHourlyStats> {
    public void Configure(EntityTypeBuilder<EntityHourlyStats> builder) {
        builder.ToTable("entity_hourly_stats");

        builder.HasKey(e => new { e.EntityType, e.EntityId, e.BucketHour });

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.BucketHour)
            .HasColumnName("bucket_hour")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.EventCount)
            .HasColumnName("event_count")
            .IsRequired();

        builder.Property(e => e.MaxSpeedKmh)
            .HasColumnName("max_speed_kmh")
            .IsRequired();

        builder.Property(e => e.SpeedSum)
            .HasColumnName("speed_sum")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}

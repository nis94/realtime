using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovementIntel.Domain.Entities;

namespace MovementIntel.Domain.Configuration;

public class RawMovementEventConfiguration : IEntityTypeConfiguration<RawMovementEvent> {
    public void Configure(EntityTypeBuilder<RawMovementEvent> builder) {
        builder.ToTable("raw_movement_events");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId)
            .HasColumnName("event_id");

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .HasColumnName("timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.Latitude)
            .HasColumnName("latitude")
            .IsRequired();

        builder.Property(e => e.Longitude)
            .HasColumnName("longitude")
            .IsRequired();

        builder.Property(e => e.SpeedKmh)
            .HasColumnName("speed_kmh");

        builder.Property(e => e.Source)
            .HasColumnName("source")
            .HasMaxLength(50);

        builder.Property(e => e.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(e => e.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
        builder.HasIndex(e => e.Timestamp);
    }
}

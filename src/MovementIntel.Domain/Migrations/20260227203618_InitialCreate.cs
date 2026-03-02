using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovementIntel.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entity_hourly_stats",
                columns: table => new
                {
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bucket_hour = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    event_count = table.Column<int>(type: "integer", nullable: false),
                    max_speed_kmh = table.Column<double>(type: "double precision", nullable: false),
                    speed_sum = table.Column<double>(type: "double precision", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_hourly_stats", x => new { x.entity_type, x.entity_id, x.bucket_hour });
                });

            migrationBuilder.CreateTable(
                name: "raw_movement_events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    speed_kmh = table.Column<double>(type: "double precision", nullable: true),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    attributes = table.Column<string>(type: "jsonb", nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_movement_events", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_raw_movement_events_entity_type_entity_id_timestamp",
                table: "raw_movement_events",
                columns: new[] { "entity_type", "entity_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_raw_movement_events_timestamp",
                table: "raw_movement_events",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_hourly_stats");

            migrationBuilder.DropTable(
                name: "raw_movement_events");
        }
    }
}

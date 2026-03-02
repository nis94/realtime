# MovementIntel - Real-Time Movement Intelligence Platform

Ingestion pipeline for movement events: consumes from Kafka, validates, stores raw events in PostgreSQL, computes hourly aggregations, and tracks last known positions in Redis. See `SYSTEM_DESIGN.md` for the full architecture and design decisions.

## Quick Start

```bash
# Only prerequisite: Docker
docker compose up --build
```

This starts PostgreSQL, Redis, Kafka, 1 API instance (for testing), and 1 processor instance. Migrations run automatically.

To scale processor instances (one consumer per Kafka partition):

```bash
docker compose up --build --scale processor=4
```

### Run Tests

```bash
# Requires .NET 9 SDK
dotnet test
```

---

## Try It

A REST endpoint is provided for testing. Events are produced to Kafka and flow through the full pipeline:

```
POST /api/v1/events → Kafka → consumer → validate → PostgreSQL + Redis
```

### Event Schema

| Field | Type | Required | Validation |
|---|---|---|---|
| `event_id` | string (UUID) | Yes | Valid UUID |
| `entity.type` | string | Yes | Non-empty |
| `entity.id` | string | Yes | Non-empty |
| `timestamp` | string (ISO 8601) | Yes | Parseable UTC datetime |
| `position.lat` | number | Yes | -90 to 90 |
| `position.lon` | number | Yes | -180 to 180 |
| `speed_kmh` | number | No | >= 0 if provided |
| `source` | string | No | Free-text |
| `attributes` | object | No | Arbitrary JSON |

### Ingest Events

```bash
curl -s -X POST http://localhost:5000/api/v1/events \
  -H "Content-Type: application/json" \
  -d '[{
    "event_id": "550e8400-e29b-41d4-a716-446655440000",
    "entity": { "type": "vehicle", "id": "v-123" },
    "timestamp": "2025-01-01T10:15:00Z",
    "position": { "lat": 52.52, "lon": 13.405 },
    "speed_kmh": 42.3,
    "source": "gps",
    "attributes": { "battery_level": 0.82 }
  }]'
```

Response: `202 Accepted` - `{ "produced": 1, "total": 1 }`. Processing is asynchronous (Kafka consumer).

### Verify Data

```bash
# Raw events in PostgreSQL
docker compose exec postgres psql -U movementintel -c "SELECT * FROM raw_movement_events;"

# Hourly aggregations in PostgreSQL
docker compose exec postgres psql -U movementintel -c "SELECT * FROM entity_hourly_stats;"

# Last known position in Redis
docker compose exec redis redis-cli HGETALL "position:vehicle:v-123"
```

---

## Assumptions & Simplifications

This implementation focuses on the ingestion pipeline - receive, validate, deduplicate, store, and derive. Several decisions were intentionally simplified relative to the full system design described in `SYSTEM_DESIGN.md`.

### Kafka as the single ingestion boundary

The system design has Kafka as the ingestion boundary: a collection team produces events to a topic, and N processor instances consume from it as a consumer group. This is implemented as designed - the `KafkaConsumerService` consumes from the `movement-events` topic, batches messages, and calls `IEventIngestionService.IngestAsync()`.

The REST endpoint (`POST /api/v1/events`) is a convenience for reviewer testing only - it produces events to Kafka rather than processing them directly, so every event flows through the same pipeline. In production, the collection team would produce to Kafka directly; this endpoint wouldn't exist.

### Single codebase, split deployment

The code lives in one project (`MovementIntel.Processor`), but docker-compose deploys it as two roles: an `api` service (REST facade, Kafka producer, consumer disabled) and a `processor` service (4 replicas, Kafka consumer enabled, no exposed port). Toggling `Kafka:EnableConsumer` determines which role the process plays.

### Deduplication via DB constraint (no in-memory cache)

Duplicates are handled by the PostgreSQL `UNIQUE` constraint on `event_id` - the batch INSERT uses `ON CONFLICT DO NOTHING`, and `RETURNING event_id` tells us which events were actually new. This is simple and correct. The design describes an in-memory dedup cache as a production optimization to reduce DB round-trips, but for this scope the DB constraint is sufficient and avoids extra code.

### Per-event derived data writes (not batched)

The raw event INSERT is already batched (one multi-row `INSERT ... VALUES` per Kafka batch). However, the derived data writes - Redis position updates and PostgreSQL aggregation upserts - are per-event. In production, both should be batched: grouped upserts for aggregations, pipelined Redis commands. I chose not to implement this because it adds complexity that obscures the core pipeline logic - the optimization path is described in `SYSTEM_DESIGN.md`.

### No dead-letter topic for invalid events

Deserialization errors and validation failures are logged and skipped - invalid events are excluded from storage but don't block the batch. In production, a dead-letter topic would capture these for investigation.

---

## What I'd Add with More Time

- **Query API endpoints** - history, stats, and bulk export (currently only last-known-position is exposed; the design doc describes the full query interface)
- **Table partitioning** - partition `raw_movement_events` by timestamp range for efficient time-based queries and data retention
- **Dead-letter topic** - route deserialization and validation failures to a separate Kafka topic for investigation
- **Batched derived data writes** - pipeline Redis position updates and group aggregation upserts per batch instead of per-event
- **Generic consumer/handler pattern** - extract Kafka infrastructure (polling, deserialization, offset management) into a reusable `KafkaConsumer<T>` with an `IMessageHandler<T>` interface, so adding new consumers/topics is plug-and-play without duplicating boilerplate

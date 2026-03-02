using Microsoft.EntityFrameworkCore;
using MovementIntel.Domain.Configuration;
using MovementIntel.Domain.Entities;

namespace MovementIntel.Domain;

public class MovementIntelDbContext(DbContextOptions<MovementIntelDbContext> options) : DbContext(options) {
    public DbSet<RawMovementEvent> RawMovementEvents => Set<RawMovementEvent>();
    public DbSet<EntityHourlyStats> EntityHourlyStats => Set<EntityHourlyStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new RawMovementEventConfiguration());
        modelBuilder.ApplyConfiguration(new EntityHourlyStatsConfiguration());
    }
}

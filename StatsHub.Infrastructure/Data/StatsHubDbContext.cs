using Microsoft.EntityFrameworkCore;
using StatsHub.Domain.Entities;

namespace StatsHub.Infrastructure.Data;

public class StatsHubDbContext : DbContext
{
    public StatsHubDbContext(DbContextOptions<StatsHubDbContext> opts) : base(opts)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(cfg =>
        {
            cfg.HasKey(o => o.Id);
            cfg.HasIndex(o => o.OrderId).IsUnique();
            cfg.Property(o => o.Price);
            cfg.Property(o => o.BrandName).HasMaxLength(100);
        });
    }
}

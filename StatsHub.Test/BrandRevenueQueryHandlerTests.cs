using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StatsHub.Application.Handlers;
using StatsHub.Application.Queries;
using StatsHub.Domain.Entities;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Test;

public class BrandRevenueQueryHandlerTests
{
    private static IDistributedCache CreateInMemoryDistributedCache()
    {
        var opts = Options.Create(new MemoryDistributedCacheOptions());
        return new MemoryDistributedCache(opts);
    }

    [Fact]
    public async Task Returns_correct_sums_per_brand()
    {
        var options = new DbContextOptionsBuilder<StatsHubDbContext>()
            .UseInMemoryDatabase("brand-revenue")
            .Options;

        await using var db = new StatsHubDbContext(options);

        db.Orders.AddRange(
            new Order { BrandName = "Nike", Price = 100, Quantity = 2 },
            new Order { BrandName = "Nike", Price = 50, Quantity = 1 },
            new Order { BrandName = "Adidas", Price = 80, Quantity = 3 }
        );

        await db.SaveChangesAsync();

        var handler = new BrandRevenueQueryHandler(db, CreateInMemoryDistributedCache(), Mock.Of<ILogger<BrandRevenueQueryHandler>>());

        var result = await handler.Handle(new BrandRevenueQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(250m, result["Nike"]);
        Assert.Equal(240m, result["Adidas"]);
    }
}

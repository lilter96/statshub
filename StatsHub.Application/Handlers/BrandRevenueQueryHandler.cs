using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StatsHub.Application.Queries;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class BrandRevenueQueryHandler : IRequestHandler<BrandRevenueQuery, IDictionary<string, decimal>>
{
    private readonly IDistributedCache _cache;
    private readonly StatsHubDbContext _db;

    public BrandRevenueQueryHandler(StatsHubDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IDictionary<string, decimal>> Handle(BrandRevenueQuery request, CancellationToken ct)
    {
        const string cacheKey = "BrandRevenue";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (cached is not null)
        {
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(cached)!;
        }

        var revenue = await _db.Orders
            .GroupBy(o => o.BrandName)
            .Select(g => new { g.Key, Revenue = g.Sum(x => x.Price * x.Quantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Revenue, ct);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        var json = JsonSerializer.Serialize(revenue);
        await _cache.SetStringAsync(cacheKey, json, options, ct);

        return revenue;
    }
}

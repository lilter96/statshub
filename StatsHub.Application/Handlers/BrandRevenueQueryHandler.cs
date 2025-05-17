using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StatsHub.Application.Queries;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class BrandRevenueQueryHandler : IRequestHandler<BrandRevenueQuery, IDictionary<string, decimal>>
{
    private const string CacheKey = nameof(BrandRevenueQueryHandler);
    private readonly IDistributedCache _cache;
    private readonly StatsHubDbContext _db;
    private readonly ILogger<BrandRevenueQueryHandler> _logger;

    public BrandRevenueQueryHandler(
        StatsHubDbContext db,
        IDistributedCache cache,
        ILogger<BrandRevenueQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IDictionary<string, decimal>> Handle(BrandRevenueQuery request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogDebug("Handling BrandRevenueQuery");

        var cached = await TryGetFromCacheAsync(ct);
        if (cached is not null)
        {
            _logger.LogDebug("Brand revenue loaded from cache");
            return cached;
        }

        var revenue = await ComputeRevenueAsync(ct);

        await SetCacheAsync(revenue, ct);
        _logger.LogInformation("Computed brand revenue for {Count} brands", revenue.Count);

        return revenue;
    }

    private async Task<IDictionary<string, decimal>?> TryGetFromCacheAsync(CancellationToken ct)
    {
        _logger.LogDebug("Checking cache for key {CacheKey}", CacheKey);
        var json = await _cache.GetStringAsync(CacheKey, ct);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cache entry for {CacheKey}", CacheKey);
            return null;
        }
    }

    private async Task<Dictionary<string, decimal>> ComputeRevenueAsync(CancellationToken ct)
    {
        var data = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => o.BrandName)
            .Select(g => new { Brand = g.Key, Revenue = g.Sum(x => x.Price * x.Quantity) })
            .ToListAsync(ct);

        return data.ToDictionary(x => x.Brand, x => x.Revenue);
    }

    private async Task SetCacheAsync(IDictionary<string, decimal> revenue, CancellationToken ct)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        var json = JsonSerializer.Serialize(revenue);
        await _cache.SetStringAsync(CacheKey, json, options, ct);
        _logger.LogDebug("Brand revenue cached with key {CacheKey}", CacheKey);
    }
}

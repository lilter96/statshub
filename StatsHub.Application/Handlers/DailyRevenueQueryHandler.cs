using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StatsHub.Application.Queries;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class DailyRevenueQueryHandler
    : IRequestHandler<DailyRevenueQuery, IReadOnlyCollection<DailyRevenueDto>>
{
    private const string CacheKey = nameof(DailyRevenueQueryHandler);
    private readonly IDistributedCache _cache;
    private readonly StatsHubDbContext _db;
    private readonly ILogger<DailyRevenueQueryHandler> _logger;

    public DailyRevenueQueryHandler(
        StatsHubDbContext db,
        IDistributedCache cache,
        ILogger<DailyRevenueQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<DailyRevenueDto>> Handle(
        DailyRevenueQuery request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogDebug("Handling {Handler}", CacheKey);

        var json = await _cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(json))
        {
            _logger.LogDebug("{Handler} data loaded from cache", CacheKey);
            return JsonSerializer.Deserialize<List<DailyRevenueDto>>(json)!;
        }

        var raw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.Price * x.Quantity)
            })
            .ToListAsync(ct);

        var result = raw
            .Select(x => new DailyRevenueDto(DateOnly.FromDateTime(x.Date), x.Revenue))
            .OrderBy(x => x.Date)
            .ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(result), options, ct);
        _logger.LogInformation("{Handler} cached for {Count} days", CacheKey, result.Count);

        return result;
    }
}

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace StatsHub.Infrastructure.Data;

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(
        IDistributedCache cache,
        ILogger<CacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task InvalidateAsync(string[] handlerNames, CancellationToken ct = default)
    {
        foreach (var name in handlerNames)
        {
            await _cache.RemoveAsync(name, ct);
            _logger.LogDebug("Cache entry removed for handler key '{Key}'", name);
        }
    }
}

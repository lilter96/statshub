namespace StatsHub.Infrastructure.Data;

public interface ICacheInvalidationService
{
    Task InvalidateAsync(string[] keys, CancellationToken ct = default);
}

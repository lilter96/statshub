using MediatR;
using Microsoft.EntityFrameworkCore;
using StatsHub.Application.Queries;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class DailyRevenueQueryHandler : IRequestHandler<DailyRevenueQuery, IReadOnlyCollection<DailyRevenueDto>>
{
    private readonly StatsHubDbContext _db;

    public DailyRevenueQueryHandler(StatsHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<DailyRevenueDto>> Handle(DailyRevenueQuery request, CancellationToken ct)
    {
        var raw = await _db.Orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                g.Key,
                Revenue = g.Sum(x => x.Price * x.Quantity)
            })
            .OrderBy(x => x.Key)
            .ToListAsync(ct);

        return raw.Select(x => new DailyRevenueDto(
                DateOnly.FromDateTime(x.Key),
                x.Revenue))
            .ToList();
    }
}

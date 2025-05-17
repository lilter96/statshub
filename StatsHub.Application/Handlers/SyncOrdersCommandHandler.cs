using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StatsHub.Application.Commands;
using StatsHub.Application.Notifications;
using StatsHub.Application.Queries;
using StatsHub.Domain.Entities;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class SyncOrdersCommandHandler : IRequestHandler<SyncOrdersCommand, int>
{
    private readonly StatsHubDbContext _db;
    private readonly ILogger<SyncOrdersCommandHandler> _log;
    private readonly IMediator _mediator;

    public SyncOrdersCommandHandler(StatsHubDbContext db, ILogger<SyncOrdersCommandHandler> log, IMediator mediator)
    {
        _db = db;
        _log = log;
        _mediator = mediator;
    }

    public async Task<int> Handle(SyncOrdersCommand request, CancellationToken ct)
    {
        var incoming = request.Orders
            .Select(o => new Order
            {
                OrderId = o.OrderId,
                Sku = o.Sku,
                Price = o.Price,
                Quantity = o.Quantity,
                CreatedAt = o.CreatedAt,
                BrandName = o.BrandName
            })
            .ToList();

        // fast-deduplication
        var incIds = incoming.Select(o => o.OrderId).ToHashSet();
        var existingIds = await _db.Orders
            .Where(o => incIds.Contains(o.OrderId))
            .Select(o => o.OrderId)
            .ToListAsync(ct);

        var fresh = incoming.Where(o => !existingIds.Contains(o.OrderId)).ToList();
        await _db.Orders.AddRangeAsync(fresh, ct);
        var saved = await _db.SaveChangesAsync(ct);

        _log.LogInformation("Sync imported {Count} new orders", saved);

        var daily = await _db.Orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueDto(DateOnly.FromDateTime(g.Key), g.Sum(x => x.Price * x.Quantity)))
            .ToListAsync(ct);

        await _mediator.Publish(new OrdersSyncedNotification(daily), ct);
        return saved;
    }
}

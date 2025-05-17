using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StatsHub.Application.Commands;
using StatsHub.Application.DTOs;
using StatsHub.Application.Notifications;
using StatsHub.Application.Queries;
using StatsHub.Domain.Entities;
using StatsHub.Infrastructure.Data;

namespace StatsHub.Application.Handlers;

public sealed class SyncOrdersCommandHandler
    : IRequestHandler<SyncOrdersCommand, int>
{
    private readonly ICacheInvalidationService _cacheInvalidator;
    private readonly StatsHubDbContext _db;
    private readonly ILogger<SyncOrdersCommandHandler> _logger;
    private readonly IMediator _mediator;

    public SyncOrdersCommandHandler(
        StatsHubDbContext db,
        ICacheInvalidationService cacheInvalidator,
        ILogger<SyncOrdersCommandHandler> logger,
        IMediator mediator)
    {
        _db = db;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<int> Handle(SyncOrdersCommand request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var incoming = MapToEntities(request.Orders);
        if (!incoming.Any())
        {
            _logger.LogInformation("No orders to sync.");
            return 0;
        }

        var fresh = await FilterNewOrdersAsync(incoming, ct);
        if (!fresh.Any())
        {
            _logger.LogInformation("All orders already exist. Nothing to save.");
            return 0;
        }

        await _db.Orders.AddRangeAsync(fresh, ct);
        var savedCount = await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Imported {Count} new orders.", savedCount);

        await _cacheInvalidator.InvalidateAsync(new[]
        {
            nameof(DailyRevenueQueryHandler),
            nameof(BrandRevenueQueryHandler)
        }, ct);

        _logger.LogDebug("Cleared cache for handlers: {Keys}",
            string.Join(", ", nameof(DailyRevenueQueryHandler), nameof(BrandRevenueQueryHandler)));

        var dailyStats = await _mediator.Send(new DailyRevenueQuery(), ct);
        await _mediator.Publish(new OrdersSyncedNotification(dailyStats), ct);

        return savedCount;
    }

    private static List<Order> MapToEntities(IEnumerable<OrderDto> dtos)
    {
        return dtos.Select(o => new Order
        {
            OrderId = o.OrderId,
            Sku = o.Sku,
            Price = o.Price,
            Quantity = o.Quantity,
            CreatedAt = DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc),
            BrandName = o.BrandName
        }).ToList();
    }

    private async Task<List<Order>> FilterNewOrdersAsync(
        List<Order> incoming,
        CancellationToken ct)
    {
        var ids = incoming.Select(x => x.OrderId).ToHashSet();
        var existing = await _db.Orders
            .AsNoTracking()
            .Where(o => ids.Contains(o.OrderId))
            .Select(o => o.OrderId)
            .ToListAsync(ct);

        return incoming
            .Where(o => !existing.Contains(o.OrderId))
            .ToList();
    }
}

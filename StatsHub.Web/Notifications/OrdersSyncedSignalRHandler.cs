using MediatR;
using Microsoft.AspNetCore.SignalR;
using StatsHub.Application.Notifications;
using Web.Hubs;

namespace Web.Notifications;

public class OrdersSyncedSignalRHandler
    : INotificationHandler<OrdersSyncedNotification>
{
    private readonly IHubContext<RevenueHub> _hub;
    private readonly ILogger<OrdersSyncedSignalRHandler> _logger;

    public OrdersSyncedSignalRHandler(IHubContext<RevenueHub> hub, ILogger<OrdersSyncedSignalRHandler> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Handle(OrdersSyncedNotification n, CancellationToken ct)
    {
        _logger.LogInformation("Sending chart update to all clients");
        await _hub.Clients.All.SendAsync("update", n.DailyStats, ct);
    }
}

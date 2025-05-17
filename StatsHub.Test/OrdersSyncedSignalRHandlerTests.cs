using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StatsHub.Application.Notifications;
using StatsHub.Application.Queries;
using Web.Hubs;
using Web.Notifications;

namespace StatsHub.Test;

public class OrdersSyncedSignalRHandlerTests
{
    [Fact]
    public async Task Sends_update_to_all_clients()
    {
        var hubClientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        hubClientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var hubContextMock = new Mock<IHubContext<RevenueHub>>();
        hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

        var handler =
            new OrdersSyncedSignalRHandler(hubContextMock.Object, Mock.Of<ILogger<OrdersSyncedSignalRHandler>>());

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyStats = new List<DailyRevenueDto>
        {
            new(now, 100m),
            new(now.AddDays(1), 200m)
        };

        await handler.Handle(new OrdersSyncedNotification(dailyStats), CancellationToken.None);

        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "update",
                It.Is<object[]>(o =>
                    o.Length == 1
                    && ReferenceEquals(o[0], dailyStats)
                ),
                CancellationToken.None
            ),
            Times.Once);
    }
}

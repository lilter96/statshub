using MediatR;
using StatsHub.Application.Queries;

namespace StatsHub.Application.Notifications;

public record OrdersSyncedNotification(
    IReadOnlyCollection<DailyRevenueDto> DailyStats) : INotification;

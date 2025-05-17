using MediatR;

namespace StatsHub.Application.Queries;

public sealed record DailyRevenueQuery : IRequest<IReadOnlyCollection<DailyRevenueDto>>;

public record DailyRevenueDto(DateOnly Date, decimal Revenue);

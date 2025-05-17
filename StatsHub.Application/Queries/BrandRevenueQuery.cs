using MediatR;

namespace StatsHub.Application.Queries;

/// key=brand, value=revenue
public sealed record BrandRevenueQuery : IRequest<IDictionary<string, decimal>>;

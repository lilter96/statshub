using MediatR;
using StatsHub.Application.DTOs;

namespace StatsHub.Application.Commands;

public sealed record SyncOrdersCommand(IReadOnlyCollection<OrderDto> Orders) : IRequest<int>;

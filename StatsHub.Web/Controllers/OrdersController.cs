using MediatR;
using Microsoft.AspNetCore.Mvc;
using StatsHub.Application.Commands;
using StatsHub.Application.DTOs;
using StatsHub.Application.Queries;

namespace Web.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Импорт заказов</summary>
    [HttpPost("sync")]
    public async Task<ActionResult<int>> Sync([FromBody] IReadOnlyCollection<OrderDto> orders,
        CancellationToken ct)
    {
        var created = await _mediator.Send(new SyncOrdersCommand(orders), ct);
        return Ok(created);
    }

    /// <summary>Сводка выручки по брендам</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<IDictionary<string, decimal>>> BrandStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BrandRevenueQuery(), ct));
    }

    /// <summary>Сводка выручки по дням</summary>
    [HttpGet("daily-stats")]
    public async Task<ActionResult<IReadOnlyCollection<DailyRevenueDto>>> DailyStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new DailyRevenueQuery(), ct));
    }
}
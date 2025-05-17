using FluentValidation;
using StatsHub.Application.DTOs;

namespace StatsHub.Application.Validators;

public class OrderDtoValidator : AbstractValidator<OrderDto>
{
    public OrderDtoValidator()
    {
        RuleFor(o => o.OrderId).NotEmpty();
        RuleFor(o => o.Sku).NotEmpty();
        RuleFor(o => o.Price).GreaterThan(0);
        RuleFor(o => o.Quantity).GreaterThan(0);
        RuleFor(o => o.CreatedAt).LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(1));
        RuleFor(o => o.BrandName).NotEmpty();
    }
}

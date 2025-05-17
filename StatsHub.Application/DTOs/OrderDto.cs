namespace StatsHub.Application.DTOs;

public record OrderDto(
    string OrderId,
    string Sku,
    decimal Price,
    int Quantity,
    DateTime CreatedAt,
    string BrandName);

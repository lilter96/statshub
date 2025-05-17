namespace StatsHub.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }

    public string OrderId { get; set; }

    public string Sku { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public string BrandName { get; set; }

    public decimal Revenue => Price * Quantity;
}

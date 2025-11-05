namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Represents an item in a basket checkout event.
/// </summary>
public record BasketItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

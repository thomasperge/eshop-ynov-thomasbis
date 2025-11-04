using BuildingBlocks.Exceptions;

namespace Ordering.Application.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an order cannot be found in the system.
/// </summary>
public class OrderNotFoundException : NotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderNotFoundException"/> class.
    /// </summary>
    /// <param name="orderId">The identifier of the order that was not found.</param>
    public OrderNotFoundException(Guid orderId) : base("commande", orderId) { }
}

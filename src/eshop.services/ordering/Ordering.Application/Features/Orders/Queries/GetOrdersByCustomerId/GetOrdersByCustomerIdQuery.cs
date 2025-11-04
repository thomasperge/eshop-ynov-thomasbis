using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

/// <summary>
/// Represents a query to retrieve orders associated with a specific customer.
/// </summary>
/// <param name="CustomerId">The unique identifier of the customer whose orders are being retrieved.</param>
public record GetOrdersByCustomerIdQuery(Guid CustomerId) : IQuery<GetOrdersByCustomerIdQueryResult>;

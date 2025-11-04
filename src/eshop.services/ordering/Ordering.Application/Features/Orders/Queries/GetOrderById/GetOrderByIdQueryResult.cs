using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Represents the result of a query to retrieve an order by ID.
/// </summary>
/// <param name="Order">The order details, or null if not found.</param>
public record GetOrderByIdQueryResult(OrderDto? Order);

using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

/// <summary>
/// Represents the result of a query to retrieve orders by customer ID.
/// </summary>
/// <param name="Orders">The collection of orders associated with the specified customer.</param>
public record GetOrdersByCustomerIdQueryResult(IEnumerable<OrderDto> Orders);

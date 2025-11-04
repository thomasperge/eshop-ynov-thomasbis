using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Represents the result of a query to retrieve orders by name.
/// </summary>
/// <param name="Orders">The collection of orders that match the specified name.</param>
public record GetOrdersByNameQueryResult(IEnumerable<OrderDto> Orders);

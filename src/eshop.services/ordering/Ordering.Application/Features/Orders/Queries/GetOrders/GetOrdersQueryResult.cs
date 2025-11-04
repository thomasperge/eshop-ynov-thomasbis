using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Represents the result of a query to retrieve a paginated list of orders.
/// </summary>
/// <param name="Orders">The collection of orders for the requested page.</param>
/// <param name="PageIndex">The zero-based index of the page returned.</param>
/// <param name="PageSize">The number of orders per page.</param>
/// <param name="TotalCount">The total number of orders available.</param>
public record GetOrdersQueryResult(
    IEnumerable<OrderDto> Orders,
    int PageIndex,
    int PageSize,
    int TotalCount);

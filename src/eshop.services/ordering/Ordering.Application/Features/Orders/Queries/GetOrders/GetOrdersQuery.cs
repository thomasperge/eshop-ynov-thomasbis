using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Represents a query to retrieve a paginated list of orders.
/// </summary>
/// <param name="PageIndex">The zero-based index of the page to retrieve.</param>
/// <param name="PageSize">The number of orders to include in each page of results.</param>
public record GetOrdersQuery(int PageIndex, int PageSize) : IQuery<GetOrdersQueryResult>;

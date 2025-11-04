using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Represents a query to retrieve a single order by its unique identifier.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to retrieve.</param>
public record GetOrderByIdQuery(Guid OrderId) : IQuery<GetOrderByIdQueryResult>;

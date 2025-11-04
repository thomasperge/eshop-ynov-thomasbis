using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Represents a query to retrieve orders filtered by order name.
/// </summary>
/// <param name="Name">The name used to filter the orders.</param>
public record GetOrdersByNameQuery(string Name) : IQuery<GetOrdersByNameQueryResult>;

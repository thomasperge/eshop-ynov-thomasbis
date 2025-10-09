using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Represents a query to get all products that belong to a specific category.
/// </summary>
/// <param name="Category">The category name to filter products by.</param>
public record GetProductByCategoryQuery(string Category) : IQuery<GetProductByCategoryQueryResult>;
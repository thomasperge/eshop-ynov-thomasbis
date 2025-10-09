using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Represents the result of the GetProductByCategory query.
/// </summary>
public record GetProductByCategoryQueryResult(IEnumerable<Product> Products);
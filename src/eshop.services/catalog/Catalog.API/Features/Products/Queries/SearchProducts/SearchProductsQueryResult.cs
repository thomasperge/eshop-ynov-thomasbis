using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.SearchProducts;

/// <summary>
/// Represents the result of the SearchProducts query execution.
/// </summary>
/// <param name="Products">The list of products matching the search criteria.</param>
/// <param name="TotalCount">The total number of products matching the criteria (before pagination).</param>
/// <param name="PageNumber">The current page number.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalPages">The total number of pages.</param>
public record SearchProductsQueryResult(
    List<Product> Products,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);


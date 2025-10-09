using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.SearchProducts;

/// <summary>
/// Represents the query to search products with multiple optional filters.
/// </summary>
/// <param name="SearchTerm">Optional search term to filter by name or description.</param>
/// <param name="Categories">Optional list of categories to filter by.</param>
/// <param name="MinPrice">Optional minimum price filter.</param>
/// <param name="MaxPrice">Optional maximum price filter.</param>
/// <param name="PageNumber">The page number for pagination (default: 1).</param>
/// <param name="PageSize">The number of items per page (default: 10).</param>
/// <param name="SortBy">Field to sort by: Name, Price, or Date (default: Name).</param>
/// <param name="SortOrder">Sort order: Asc or Desc (default: Asc).</param>
public record SearchProductsQuery(
    string? SearchTerm = null,
    List<string>? Categories = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "Name",
    string SortOrder = "Asc") : IQuery<SearchProductsQueryResult>;


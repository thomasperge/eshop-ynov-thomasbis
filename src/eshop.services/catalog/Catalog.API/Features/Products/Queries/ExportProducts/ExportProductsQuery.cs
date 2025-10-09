using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Represents the query to export products to an Excel file with optional filters.
/// </summary>
/// <param name="SearchTerm">Optional search term to filter by name or description.</param>
/// <param name="Categories">Optional list of categories to filter by.</param>
/// <param name="MinPrice">Optional minimum price filter.</param>
/// <param name="MaxPrice">Optional maximum price filter.</param>
/// <param name="SortBy">Field to sort by: Name, Price, or Date (default: Name).</param>
/// <param name="SortOrder">Sort order: Asc or Desc (default: Asc).</param>
public record ExportProductsQuery(
    string? SearchTerm = null,
    List<string>? Categories = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "Name",
    string SortOrder = "Asc") : IQuery<ExportProductsQueryResult>;


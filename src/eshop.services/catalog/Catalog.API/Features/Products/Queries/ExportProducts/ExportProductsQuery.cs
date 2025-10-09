using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Represents the query to export all products to an Excel file.
/// </summary>
public record ExportProductsQuery() : IQuery<ExportProductsQueryResult>;


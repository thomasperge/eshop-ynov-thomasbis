namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Represents the result of the ExportProducts query execution.
/// </summary>
/// <param name="FileContent">The binary content of the Excel file.</param>
/// <param name="FileName">The name of the Excel file.</param>
/// <param name="ContentType">The MIME type of the Excel file.</param>
/// <param name="TotalProductsExported">The total number of products exported.</param>
public record ExportProductsQueryResult(
    byte[] FileContent,
    string FileName,
    string ContentType,
    int TotalProductsExported);


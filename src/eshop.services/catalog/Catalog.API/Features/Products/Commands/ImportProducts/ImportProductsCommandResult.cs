namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Represents the result of the ImportProducts command execution.
/// </summary>
/// <param name="TotalProcessed">Total number of rows processed from the Excel file.</param>
/// <param name="SuccessfullyImported">Number of products successfully imported.</param>
/// <param name="FailedImports">Number of products that failed to import.</param>
/// <param name="Errors">List of error messages for failed imports.</param>
public record ImportProductsCommandResult(
    int TotalProcessed,
    int SuccessfullyImported,
    int FailedImports,
    List<string> Errors);

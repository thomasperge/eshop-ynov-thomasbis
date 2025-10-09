using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Handles the ImportProducts command to import products from an Excel file.
/// </summary>
public class ImportProductsCommandHandler(IDocumentSession documentSession) : ICommandHandler<ImportProductsCommand, ImportProductsCommandResult>
{
    /// <summary>
    /// Handles the processing of the ImportProducts command, which reads an Excel file and imports products.
    /// </summary>
    /// <param name="request">The ImportProducts command containing the Excel file.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the result of the import process.</returns>
    public async Task<ImportProductsCommandResult> Handle(ImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var successfullyImported = 0;
        var totalProcessed = 0;

        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(request.ExcelFile.OpenReadStream());
        var worksheet = package.Workbook.Worksheets[0];

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        
        // Skip header row (row 1)
        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                totalProcessed++;
                
                var name = worksheet.Cells[row, 1].Value?.ToString();
                var description = worksheet.Cells[row, 2].Value?.ToString();
                var priceText = worksheet.Cells[row, 3].Value?.ToString();
                var imageFile = worksheet.Cells[row, 4].Value?.ToString();
                var categoriesText = worksheet.Cells[row, 5].Value?.ToString();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Row {row}: Product name is required");
                    continue;
                }

                if (!decimal.TryParse(priceText, out var price))
                {
                    errors.Add($"Row {row}: Invalid price format");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(categoriesText))
                {
                    errors.Add($"Row {row}: Categories are required");
                    continue;
                }

                // Parse categories (split by comma and trim)
                var categories = categoriesText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                if (categories.Count == 0)
                {
                    errors.Add($"Row {row}: At least one category is required");
                    continue;
                }

                // Check if product already exists
                var existingProduct = await documentSession.Query<Product>()
                    .FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase), cancellationToken);

                if (existingProduct != null)
                {
                    errors.Add($"Row {row}: Product '{name}' already exists");
                    continue;
                }

                // Create new product
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description ?? string.Empty,
                    Price = price,
                    ImageFile = imageFile ?? string.Empty,
                    Categories = categories
                };

                documentSession.Store(product);
                successfullyImported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row}: Error processing product - {ex.Message}");
            }
        }

        // Save all changes at once
        if (successfullyImported > 0)
        {
            await documentSession.SaveChangesAsync(cancellationToken);
        }

        return new ImportProductsCommandResult(
            totalProcessed,
            successfullyImported,
            errors.Count,
            errors);
    }
}

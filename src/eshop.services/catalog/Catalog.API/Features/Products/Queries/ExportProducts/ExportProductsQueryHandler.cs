using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Handles the ExportProducts query to export all products from the database to an Excel file.
/// </summary>
public class ExportProductsQueryHandler(IDocumentSession documentSession) : IQueryHandler<ExportProductsQuery, ExportProductsQueryResult>
{
    /// <summary>
    /// Handles the processing of the ExportProducts query, which reads all products and creates an Excel file.
    /// </summary>
    /// <param name="request">The ExportProducts query.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the Excel file as byte array.</returns>
    public async Task<ExportProductsQueryResult> Handle(ExportProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Récupérer tous les produits de la base de données
        var products = await documentSession.Query<Product>()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        // Configurer la licence EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Créer le fichier Excel en mémoire
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        // Définir les en-têtes (ligne 1)
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Description";
        worksheet.Cells[1, 3].Value = "Price";
        worksheet.Cells[1, 4].Value = "ImageFile";
        worksheet.Cells[1, 5].Value = "Categories";

        // Styliser les en-têtes
        using (var headerRange = worksheet.Cells[1, 1, 1, 5])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        // Remplir les données (à partir de la ligne 2)
        var row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.Name;
            worksheet.Cells[row, 2].Value = product.Description;
            worksheet.Cells[row, 3].Value = product.Price;
            worksheet.Cells[row, 4].Value = product.ImageFile;
            
            // Joindre les catégories avec des virgules
            worksheet.Cells[row, 5].Value = string.Join(", ", product.Categories);
            
            row++;
        }

        // Auto-ajuster la largeur des colonnes
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Convertir le package Excel en tableau de bytes
        var fileContent = package.GetAsByteArray();

        // Générer un nom de fichier avec la date et l'heure
        var fileName = $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return new ExportProductsQueryResult(
            fileContent,
            fileName,
            contentType,
            products.Count);
    }
}


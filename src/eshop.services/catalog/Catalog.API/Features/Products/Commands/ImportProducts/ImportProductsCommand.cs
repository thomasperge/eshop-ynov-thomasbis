using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Represents the command to import products from an Excel file.
/// </summary>
/// <param name="ExcelFile">The Excel file containing the products to import.</param>
public record ImportProductsCommand(IFormFile ExcelFile) : ICommand<ImportProductsCommandResult>;

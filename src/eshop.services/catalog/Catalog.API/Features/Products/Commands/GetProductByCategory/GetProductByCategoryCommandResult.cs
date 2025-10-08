using Catalog.API.Models;

namespace Catalog.API.Features.Products.Commands.GetProductByCategory;

/// <summary>
/// Represents the result of the GetProductByCategory query.
/// </summary>
public record GetProductByCategoryCommandResult(IEnumerable<Product> Products);
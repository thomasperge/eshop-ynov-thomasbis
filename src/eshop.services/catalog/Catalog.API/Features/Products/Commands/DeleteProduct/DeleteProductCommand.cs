using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents the command to delete an existing product.
/// </summary>
/// <param name="Id">The unique identifier of the product to delete.</param>
public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResult>;

using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Represents a command to update an existing product in the catalog.
/// </summary>
/// <remarks>
/// This command encapsulates all the information required to update a product's details,
/// including its ID, name, description, price, associated image file, and category list.
/// </remarks>
/// <param name="Id">The unique identifier of the product to be updated.</param>
/// <param name="Name">The new name of the product.</param>
/// <param name="Description">The updated description for the product.</param>
/// <param name="Price">The new price of the product.</param>
/// <param name="ImageFile">The name or path of the image file associated with the product.</param>
/// <param name="Categories">A list of categories to which the product belongs.</param>
/// <param name="Stock">The available stock quantity for the product.</param>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string ImageFile,
    List<string> Categories,
    int Stock = 0)
    : ICommand<UpdateProductCommandResult>;
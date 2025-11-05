using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.CreateProduct;

/// <summary>
/// Represents the command to create a new product.
/// </summary>
public class CreateProductCommand : ICommand<CreateProductCommandResult>
{
    /// <summary>
    /// Gets or sets the name of the product being created.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product being created.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product being created.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the image file associated with the product being created.
    /// </summary>
    public string ImageFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of categories associated with the product being created.
    /// </summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the initial stock quantity for the product being created.
    /// </summary>
    public int Stock { get; set; } = 0;
}
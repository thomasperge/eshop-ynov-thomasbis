namespace Catalog.API.Models;

/// <summary>
/// Represents a product within the catalog. Provides details such as product name, description,
/// price, associated categories, and an image file.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the name of the image file associated with the product.
    /// </summary>
    public string ImageFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of categories associated with the product.
    /// </summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the available stock quantity for the product.
    /// </summary>
    public int Stock { get; set; } = 0;
}
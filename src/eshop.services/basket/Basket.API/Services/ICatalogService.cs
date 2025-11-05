namespace Basket.API.Services;

/// <summary>
/// Service interface for interacting with Catalog.API.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Gets product details from Catalog.API, including stock information.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product details with stock, or null if not found.</returns>
    Task<CatalogProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO representing a product from Catalog.API, including stock information.
/// </summary>
public record CatalogProductDto(Guid Id, string Name, decimal Price, int Stock);


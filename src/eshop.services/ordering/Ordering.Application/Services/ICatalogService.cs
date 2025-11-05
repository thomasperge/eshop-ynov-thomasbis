namespace Ordering.Application.Services;

/// <summary>
/// Service interface for interacting with Catalog.API.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Checks if a product exists in Catalog.API.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the product exists, false otherwise.</returns>
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product details from Catalog.API.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product details, or null if not found.</returns>
    Task<CatalogProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves stock for a product in Catalog.API.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the reservation was successful, false otherwise.</returns>
    Task<bool> ReserveProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO representing a product from Catalog.API.
/// </summary>
public record CatalogProductDto(Guid Id, string Name, decimal Price);

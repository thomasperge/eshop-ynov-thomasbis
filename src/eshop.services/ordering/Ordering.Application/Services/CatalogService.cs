using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Services;

/// <summary>
/// Service implementation for interacting with Catalog.API using HttpClient.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(HttpClient httpClient, ILogger<CatalogService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a product exists in Catalog.API by calling GET /products/{id}.
    /// </summary>
    public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking if product {ProductId} exists in Catalog.API", productId);
            
            var response = await _httpClient.GetAsync($"products/{productId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Product {ProductId} exists in Catalog.API", productId);
                return true;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product {ProductId} not found in Catalog.API", productId);
                return false;
            }
            
            // Other error status codes
            _logger.LogError("Error checking product {ProductId} in Catalog.API. Status: {StatusCode}", 
                productId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking if product {ProductId} exists in Catalog.API", productId);
            return false;
        }
    }

    /// <summary>
    /// Gets product details from Catalog.API by calling GET /products/{id}.
    /// </summary>
    public async Task<CatalogProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting product details for {ProductId} from Catalog.API", productId);
            
            var response = await _httpClient.GetAsync($"products/{productId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var catalogProduct = await response.Content.ReadFromJsonAsync<CatalogProductDto>(cancellationToken);
                if (catalogProduct != null)
                {
                    _logger.LogInformation("Successfully retrieved product {ProductId} ({ProductName}) from Catalog.API", 
                        productId, catalogProduct.Name);
                    return catalogProduct;
                }
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product {ProductId} not found in Catalog.API", productId);
                return null;
            }
            
            _logger.LogError("Error getting product {ProductId} from Catalog.API. Status: {StatusCode}", 
                productId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting product {ProductId} from Catalog.API", productId);
            return null;
        }
    }

    /// <summary>
    /// Reserves stock for a product in Catalog.API.
    /// Note: This endpoint may not exist yet in Catalog.API, but the structure is prepared.
    /// </summary>
    public async Task<bool> ReserveProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reserving {Quantity} units of product {ProductId} in Catalog.API", quantity, productId);
            
            // Try to call POST /products/{id}/reserve endpoint
            var requestBody = new { Quantity = quantity };
            var response = await _httpClient.PostAsJsonAsync($"products/{productId}/reserve", requestBody, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully reserved {Quantity} units of product {ProductId}", quantity, productId);
                return true;
            }
            
            // If endpoint doesn't exist (404) or other error
            _logger.LogWarning("Failed to reserve stock for product {ProductId}. Status: {StatusCode}. " +
                             "This endpoint may not be implemented yet in Catalog.API.", 
                productId, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while reserving stock for product {ProductId} in Catalog.API", productId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while reserving stock for product {ProductId} in Catalog.API", productId);
            return false;
        }
    }
}

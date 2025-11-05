using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Basket.API.Services;

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
                    _logger.LogInformation("Successfully retrieved product {ProductId} ({ProductName}) from Catalog.API. Stock: {Stock}", 
                        productId, catalogProduct.Name, catalogProduct.Stock);
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
}


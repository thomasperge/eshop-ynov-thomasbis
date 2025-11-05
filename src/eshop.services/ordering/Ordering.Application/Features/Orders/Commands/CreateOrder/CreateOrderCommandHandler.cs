using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Services;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler (IOrderingDbContext orderingDbContext, ICatalogService catalogService, ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Ensure Customer exists - if not found, create it from shipping address info
        var requestedCustomerId = CustomerId.Of(request.Order.CustomerId);
        var customer = await orderingDbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == requestedCustomerId, cancellationToken);
        
        if (customer == null)
        {
            // Check if a customer with this email already exists (constraint unique on Email)
            var existingCustomerByEmail = await orderingDbContext.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Order.ShippingAddress.EmailAddress, cancellationToken);
            
            if (existingCustomerByEmail != null)
            {
                // Use the existing customer instead of creating a new one
                customer = existingCustomerByEmail;
            }
            else
            {
                // Create customer from shipping address email and name
                var customerName = $"{request.Order.ShippingAddress.FirstName} {request.Order.ShippingAddress.LastName}";
                customer = Customer.Create(requestedCustomerId, customerName, request.Order.ShippingAddress.EmailAddress);
                await orderingDbContext.Customers.AddAsync(customer, cancellationToken);
            }
        }
        
        // Verify that all products exist in Catalog.API before creating the order
        var requestedProductGuids = request.Order.OrderItems.Select(item => item.ProductId).Distinct().ToList();
        
        logger.LogInformation("Verifying {Count} products in Catalog.API", requestedProductGuids.Count);
        
        var missingProducts = new List<Guid>();
        
        foreach (var productId in requestedProductGuids)
        {
            var exists = await catalogService.ProductExistsAsync(productId, cancellationToken);
            if (!exists)
            {
                missingProducts.Add(productId);
                logger.LogWarning("Product {ProductId} not found in Catalog.API", productId);
            }
        }
        
        if (missingProducts.Any())
        {
            throw new InvalidOperationException(
                $"One or more products do not exist in Catalog.API. Missing ProductIds: {string.Join(", ", missingProducts)}");
        }
        
        // Ensure all products exist in the local database (Ordering.API)
        // If a product exists in Catalog.API but not in Ordering.API, create it
        var allProducts = await orderingDbContext.Products.ToListAsync(cancellationToken);
        var existingProducts = allProducts
            .Where(p => requestedProductGuids.Contains(p.Id.Value))
            .ToList();
        
        var existingProductIds = existingProducts.Select(p => p.Id.Value).ToHashSet();
        var missingProductIds = requestedProductGuids.Where(id => !existingProductIds.Contains(id)).ToList();
        
        if (missingProductIds.Any())
        {
            logger.LogInformation("Creating {Count} missing products in Ordering.API database", missingProductIds.Count);
            
            foreach (var productId in missingProductIds)
            {
                // Get product details from Catalog.API
                var catalogProduct = await catalogService.GetProductAsync(productId, cancellationToken);
                
                if (catalogProduct == null)
                {
                    logger.LogWarning("Product {ProductId} exists in Catalog.API (verified earlier) but could not be retrieved. Skipping creation in Ordering.API.", productId);
                    continue;
                }
                
                // Create the product in Ordering.API
                var product = Product.Create(
                    ProductId.Of(catalogProduct.Id),
                    catalogProduct.Name,
                    catalogProduct.Price
                );
                
                await orderingDbContext.Products.AddAsync(product, cancellationToken);
                logger.LogInformation("Created product {ProductId} ({ProductName}) in Ordering.API database", 
                    catalogProduct.Id, catalogProduct.Name);
            }
            
            // Save the new products before creating the order
            await orderingDbContext.SaveChangesAsync(cancellationToken);
        }
        
        // Reserve stock for each product in Catalog.API
        // If reservation fails (stock insufficient, product not found, etc.), throw an exception to prevent order creation
        var reservationFailures = new List<string>();
        
        foreach (var orderItem in request.Order.OrderItems)
        {
            var reserved = await catalogService.ReserveProductStockAsync(orderItem.ProductId, orderItem.Quantity, cancellationToken);
            if (!reserved)
            {
                reservationFailures.Add($"Product {orderItem.ProductId}: quantity {orderItem.Quantity}");
                logger.LogWarning("Could not reserve stock for product {ProductId}, quantity {Quantity}. " +
                                "Possible reasons: insufficient stock, product not found, or Catalog.API unavailable.", 
                    orderItem.ProductId, orderItem.Quantity);
            }
        }
        
        // If any reservation failed, throw an exception to prevent order creation
        if (reservationFailures.Any())
        {
            var errorMessage = $"Failed to reserve stock for the following products: {string.Join(", ", reservationFailures)}";
            logger.LogError("Order creation cancelled due to stock reservation failures: {ErrorMessage}", errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
        
        // Create order with the correct CustomerId (use the one from the customer we found/created)
        var order = CreateOrderCommandMapper.CreateNewOrderFromDto(request.Order, customer.Id);
        
        // Add order to context - OrderItems will be saved automatically via cascade
        await orderingDbContext.Orders.AddAsync(order, cancellationToken);
        
        // Save changes - Domain Events will be dispatched by the interceptor
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateOrderCommandResult(order.Id.Value);
    }
}
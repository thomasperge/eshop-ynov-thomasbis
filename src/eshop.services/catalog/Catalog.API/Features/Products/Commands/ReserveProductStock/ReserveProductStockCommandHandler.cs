using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.ReserveProductStock;

/// <summary>
/// Handler for reserving product stock.
/// Validates stock availability and decrements the stock when reservation is successful.
/// </summary>
public class ReserveProductStockCommandHandler(IDocumentSession documentSession, ILogger<ReserveProductStockCommandHandler> logger) 
    : ICommandHandler<ReserveProductStockCommand, ReserveProductStockCommandResult>
{
    public async Task<ReserveProductStockCommandResult> Handle(ReserveProductStockCommand request, CancellationToken cancellationToken)
    {
        // Check if product exists
        var product = await documentSession.LoadAsync<Product>(request.ProductId, cancellationToken);
        
        if (product == null)
        {
            logger.LogWarning("Product {ProductId} not found for stock reservation", request.ProductId);
            return new ReserveProductStockCommandResult(false, $"Product {request.ProductId} not found");
        }
        
        // Validate quantity
        if (request.Quantity <= 0)
        {
            logger.LogWarning("Invalid quantity {Quantity} requested for product {ProductId}", request.Quantity, request.ProductId);
            return new ReserveProductStockCommandResult(false, "Quantity must be greater than 0");
        }
        
        // Check stock availability
        if (product.Stock < request.Quantity)
        {
            logger.LogWarning("Insufficient stock for product {ProductId} ({ProductName}). Available: {AvailableStock}, Requested: {Quantity}", 
                request.ProductId, product.Name, product.Stock, request.Quantity);
            return new ReserveProductStockCommandResult(false, 
                $"Insufficient stock. Available: {product.Stock}, Requested: {request.Quantity}");
        }
        
        // Reserve stock: decrement the available stock
        product.Stock -= request.Quantity;
        
        logger.LogInformation("Stock reservation successful: Product {ProductId} ({ProductName}), Reserved: {Quantity}, Remaining: {RemainingStock}", 
            request.ProductId, product.Name, request.Quantity, product.Stock);
        
        // Save changes
        documentSession.Update(product);
        await documentSession.SaveChangesAsync(cancellationToken);
        
        return new ReserveProductStockCommandResult(true, 
            $"Stock reservation successful for product {product.Name}. Remaining stock: {product.Stock}");
    }
}

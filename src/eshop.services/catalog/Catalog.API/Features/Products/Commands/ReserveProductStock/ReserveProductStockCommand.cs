using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.ReserveProductStock;

/// <summary>
/// Command to reserve stock for a product.
/// </summary>
public record ReserveProductStockCommand(Guid ProductId, int Quantity) : ICommand<ReserveProductStockCommandResult>;

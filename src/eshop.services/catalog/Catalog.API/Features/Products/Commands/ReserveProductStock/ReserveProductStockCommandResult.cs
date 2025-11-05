namespace Catalog.API.Features.Products.Commands.ReserveProductStock;

/// <summary>
/// Result of the reserve product stock command.
/// </summary>
public record ReserveProductStockCommandResult(bool IsSuccess, string Message);

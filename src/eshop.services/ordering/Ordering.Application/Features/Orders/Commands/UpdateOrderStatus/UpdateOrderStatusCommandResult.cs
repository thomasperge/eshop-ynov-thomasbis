namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Represents the result of the execution of an update order status command.
/// </summary>
/// <param name="IsSuccess">A boolean value indicating whether the status update was successful.</param>
public record UpdateOrderStatusCommandResult(bool IsSuccess);

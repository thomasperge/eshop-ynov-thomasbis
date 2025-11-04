using BuildingBlocks.CQRS;
using Ordering.Domain.Enums;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Represents a command to update only the status of an existing order.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to update.</param>
/// <param name="OrderStatus">The new status to set for the order.</param>
public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus OrderStatus) : ICommand<UpdateOrderStatusCommandResult>;

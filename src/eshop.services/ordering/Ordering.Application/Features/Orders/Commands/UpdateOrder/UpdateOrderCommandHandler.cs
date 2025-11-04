using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Exceptions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Handles the update order command, allowing the modification of an existing order in the system.
/// This handler retrieves the specified order, updates it with new values, and persists the changes
/// to the database. If the order does not exist, an exception is thrown.
/// </summary>
public class UpdateOrderCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<UpdateOrderCommand, UpdateOrderCommandResult>
{
    public async Task<UpdateOrderCommandResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.Order.Id);
        
        var order = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.Order.Id);
        }
        
        // Update order with new values from DTO
        UpdateOrderCommandMapper.UpdateOrderWithNewValues(order, request.Order);
        
        // Save changes - Domain Events will be dispatched by the interceptor
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        
        return new UpdateOrderCommandResult(true);
    }
}
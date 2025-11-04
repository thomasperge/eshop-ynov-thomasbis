using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Exceptions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Handles the update order status command, allowing modification of only the order status.
/// </summary>
public class UpdateOrderStatusCommandHandler(IOrderingDbContext orderingDbContext) 
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusCommandResult>
{
    public async Task<UpdateOrderStatusCommandResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);
        
        var order = await orderingDbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }
        
        // Update only the status
        order.UpdateStatus(request.OrderStatus);
        
        // Save changes - Domain Events will be dispatched by the interceptor
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        
        return new UpdateOrderStatusCommandResult(true);
    }
}

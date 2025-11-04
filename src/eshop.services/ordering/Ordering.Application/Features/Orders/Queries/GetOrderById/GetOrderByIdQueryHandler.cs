using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handles the GetOrderById query to retrieve a single order by its ID.
/// </summary>
public class GetOrderByIdQueryHandler(IOrderingDbContext orderingDbContext) 
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdQueryResult>
{
    public async Task<GetOrderByIdQueryResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Check if Guid is empty to avoid DomainException
        if (request.OrderId == Guid.Empty)
        {
            return new GetOrderByIdQueryResult(Order: null);
        }

        var orderId = OrderId.Of(request.OrderId);
        
        var order = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return new GetOrderByIdQueryResult(Order: null);
        }

        var orderDto = order.ToOrderDto();

        return new GetOrderByIdQueryResult(Order: orderDto);
    }
}

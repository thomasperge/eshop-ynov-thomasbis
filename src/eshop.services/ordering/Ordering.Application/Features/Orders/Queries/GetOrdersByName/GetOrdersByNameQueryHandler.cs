using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Handles the GetOrdersByName query to retrieve orders filtered by order name.
/// </summary>
public class GetOrdersByNameQueryHandler(IOrderingDbContext orderingDbContext) 
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameQueryResult>
{
    public async Task<GetOrdersByNameQueryResult> Handle(GetOrdersByNameQuery request, CancellationToken cancellationToken)
    {
        // Use EF.Property to access the underlying Guid value for ordering
        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderName.Value.Contains(request.Name))
            .OrderByDescending(o => EF.Property<Guid>(o, "Id"))
            .ToListAsync(cancellationToken);

        var orderDtos = orders.ToOrderDtoList().ToList();

        return new GetOrdersByNameQueryResult(Orders: orderDtos);
    }
}

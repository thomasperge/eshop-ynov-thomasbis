using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Handles the GetOrders query to retrieve a paginated list of orders.
/// </summary>
public class GetOrdersQueryHandler(IOrderingDbContext orderingDbContext) 
    : IQueryHandler<GetOrdersQuery, GetOrdersQueryResult>
{
    public async Task<GetOrdersQueryResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        // Use EF.Property to access the underlying Guid value for ordering
        var orders = await query
            .OrderByDescending(o => EF.Property<Guid>(o, "Id"))
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.ToOrderDtoList().ToList();

        return new GetOrdersQueryResult(
            Orders: orderDtos,
            PageIndex: request.PageIndex,
            PageSize: request.PageSize,
            TotalCount: totalCount
        );
    }
}

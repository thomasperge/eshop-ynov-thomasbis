using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.ValueObjects;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

/// <summary>
/// Handles the GetOrdersByCustomerId query to retrieve orders for a specific customer.
/// </summary>
public class GetOrdersByCustomerIdQueryHandler(IOrderingDbContext orderingDbContext) 
    : IQueryHandler<GetOrdersByCustomerIdQuery, GetOrdersByCustomerIdQueryResult>
{
    public async Task<GetOrdersByCustomerIdQueryResult> Handle(GetOrdersByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        // Check if Guid is empty to avoid DomainException
        if (request.CustomerId == Guid.Empty)
        {
            return new GetOrdersByCustomerIdQueryResult(Orders: Enumerable.Empty<OrderDto>());
        }

        var customerId = CustomerId.Of(request.CustomerId);
        
        // Use EF.Property to access the underlying Guid value for ordering
        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => EF.Property<Guid>(o, "Id"))
            .ToListAsync(cancellationToken);

        var orderDtos = orders.ToOrderDtoList().ToList();

        return new GetOrdersByCustomerIdQueryResult(Orders: orderDtos);
    }
}

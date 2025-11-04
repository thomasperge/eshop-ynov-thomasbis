using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler (IOrderingDbContext orderingDbContext) : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Ensure Customer exists - if not found, create it from shipping address info
        var requestedCustomerId = CustomerId.Of(request.Order.CustomerId);
        var customer = await orderingDbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == requestedCustomerId, cancellationToken);
        
        if (customer == null)
        {
            // Check if a customer with this email already exists (constraint unique on Email)
            var existingCustomerByEmail = await orderingDbContext.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Order.ShippingAddress.EmailAddress, cancellationToken);
            
            if (existingCustomerByEmail != null)
            {
                // Use the existing customer instead of creating a new one
                customer = existingCustomerByEmail;
            }
            else
            {
                // Create customer from shipping address email and name
                var customerName = $"{request.Order.ShippingAddress.FirstName} {request.Order.ShippingAddress.LastName}";
                customer = Customer.Create(requestedCustomerId, customerName, request.Order.ShippingAddress.EmailAddress);
                await orderingDbContext.Customers.AddAsync(customer, cancellationToken);
            }
        }
        
        // Verify that all products exist in the database before creating the order
        var requestedProductGuids = request.Order.OrderItems.Select(item => item.ProductId).ToList();
        
        // Load all products and filter in memory to avoid EF Core Value Object translation issues
        var allProducts = await orderingDbContext.Products.ToListAsync(cancellationToken);
        var existingProducts = allProducts
            .Where(p => requestedProductGuids.Contains(p.Id.Value))
            .ToList();
        
        if (existingProducts.Count != requestedProductGuids.Count)
        {
            var existingProductIds = existingProducts.Select(p => p.Id.Value).ToHashSet();
            var missingProductIds = requestedProductGuids.Where(id => !existingProductIds.Contains(id)).ToList();
            throw new InvalidOperationException(
                $"One or more products do not exist in the database. Missing ProductIds: {string.Join(", ", missingProductIds)}");
        }
        
        // Create order with the correct CustomerId (use the one from the customer we found/created)
        var order = CreateOrderCommandMapper.CreateNewOrderFromDto(request.Order, customer.Id);
        
        // Add order to context - OrderItems will be saved automatically via cascade
        await orderingDbContext.Orders.AddAsync(order, cancellationToken);
        
        // Save changes - Domain Events will be dispatched by the interceptor
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateOrderCommandResult(order.Id.Value);
    }
}
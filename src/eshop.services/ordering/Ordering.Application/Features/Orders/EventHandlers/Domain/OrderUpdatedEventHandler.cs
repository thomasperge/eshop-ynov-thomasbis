using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Extensions;
using Ordering.Application.Services;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

public class OrderUpdatedEventHandler(
    IEmailService emailService,
    ILogger<OrderUpdatedEventHandler> logger) : INotificationHandler<OrderUpdatedEvent>
{
    public async Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        // Convert Order to DTO for email
        var orderDto = notification.Order.ToOrderDto();

        // Send email notification for specific status changes (Cancelled or Shipped)
        if (orderDto.OrderStatus == OrderStatus.Cancelled || orderDto.OrderStatus == OrderStatus.Shipped)
        {
            try
            {
                var emailSent = await emailService.SendOrderStatusUpdateEmailAsync(orderDto, cancellationToken);
                if (emailSent)
                {
                    logger.LogInformation("Order status update email sent successfully for order {OrderId} with status {Status}",
                        orderDto.Id, orderDto.OrderStatus);
                }
                else
                {
                    logger.LogWarning("Failed to send order status update email for order {OrderId} with status {Status}",
                        orderDto.Id, orderDto.OrderStatus);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - email failure shouldn't block order status update
                logger.LogError(ex, "Exception occurred while sending order status update email for order {OrderId}",
                    orderDto.Id);
            }
        }
    }
}
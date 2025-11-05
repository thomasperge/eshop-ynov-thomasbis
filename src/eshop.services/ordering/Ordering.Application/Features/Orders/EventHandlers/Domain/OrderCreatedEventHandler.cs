using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Ordering.Application.Extensions;
using Ordering.Application.Services;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

/// <summary>
/// Handles the domain event for an order being created.
/// This handler is responsible for processing the <see cref="OrderCreatedEvent"/>
/// and publishing an integration event based on the order details.
/// Also sends an email confirmation to the customer.
/// </summary>
public class OrderCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IFeatureManager featureManager,
    IEmailService emailService,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    /// <summary>
    /// Handles the domain event when a new order is created.
    /// </summary>
    /// <param name="notification">The <see cref="OrderCreatedEvent"/> containing details of the created order.</param>
    /// <param name="cancellationToken">A cancellation token to observe while performing the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        // Convert Order to DTO for email and event publishing
        var orderDto = notification.Order.ToOrderDto();

        // Send order confirmation email
        // Errors are logged but don't throw to avoid blocking the order creation process
        try
        {
            var emailSent = await emailService.SendOrderConfirmationEmailAsync(orderDto, cancellationToken);
            if (emailSent)
            {
                logger.LogInformation("Order confirmation email sent successfully for order {OrderId}", orderDto.Id);
            }
            else
            {
                logger.LogWarning("Failed to send order confirmation email for order {OrderId}", orderDto.Id);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - email failure shouldn't block order creation
            logger.LogError(ex, "Exception occurred while sending order confirmation email for order {OrderId}", orderDto.Id);
        }

        // Publish integration event if feature is enabled
        if (await featureManager.IsEnabledAsync("OrderFulfilment"))
        {
            await publishEndpoint.Publish(orderDto, cancellationToken);
        }
    }
}
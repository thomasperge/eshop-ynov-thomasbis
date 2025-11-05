using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Services;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an order confirmation email to the customer.
    /// </summary>
    /// <param name="orderDto">The order details to include in the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendOrderConfirmationEmailAsync(OrderDto orderDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email notification when the order status is updated (cancellation, shipment, etc.).
    /// </summary>
    /// <param name="orderDto">The order details to include in the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendOrderStatusUpdateEmailAsync(OrderDto orderDto, CancellationToken cancellationToken = default);
}

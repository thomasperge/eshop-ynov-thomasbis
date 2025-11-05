using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Services;
using Ordering.Domain.Enums;

namespace Ordering.Infrastructure.Services;

/// <summary>
/// Service for sending emails using SMTP (MailKit).
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends an order confirmation email to the customer.
    /// </summary>
    public async Task<bool> SendOrderConfirmationEmailAsync(OrderDto orderDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? throw new InvalidOperationException("EmailSettings:SmtpHost is not configured");
            var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 2525);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? throw new InvalidOperationException("EmailSettings:SmtpUsername is not configured");
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured");
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@eshop.com";
            var fromName = _configuration["EmailSettings:FromName"] ?? "E-Shop";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(
                $"{orderDto.ShippingAddress.FirstName} {orderDto.ShippingAddress.LastName}",
                orderDto.ShippingAddress.EmailAddress));
            message.Subject = $"Confirmation de commande - {orderDto.OrderName}";

            // Create HTML body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateOrderConfirmationHtml(orderDto)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Order confirmation email sent successfully to {Email} for order {OrderId}",
                orderDto.ShippingAddress.EmailAddress, orderDto.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId} to {Email}",
                orderDto.Id, orderDto.ShippingAddress.EmailAddress);
            return false;
        }
    }

    /// <summary>
    /// Sends an email notification when the order status is updated (cancellation, shipment, etc.).
    /// </summary>
    public async Task<bool> SendOrderStatusUpdateEmailAsync(OrderDto orderDto, CancellationToken cancellationToken = default)
    {
        // Only send emails for specific status changes
        if (orderDto.OrderStatus != OrderStatus.Cancelled && orderDto.OrderStatus != OrderStatus.Shipped)
        {
            return false; // No email needed for other status changes
        }

        try
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? throw new InvalidOperationException("EmailSettings:SmtpHost is not configured");
            var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 2525);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? throw new InvalidOperationException("EmailSettings:SmtpUsername is not configured");
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured");
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@eshop.com";
            var fromName = _configuration["EmailSettings:FromName"] ?? "E-Shop";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(
                $"{orderDto.ShippingAddress.FirstName} {orderDto.ShippingAddress.LastName}",
                orderDto.ShippingAddress.EmailAddress));

            // Set subject based on status
            message.Subject = orderDto.OrderStatus == OrderStatus.Cancelled
                ? $"Annulation de votre commande - {orderDto.OrderName}"
                : $"Expédition de votre commande - {orderDto.OrderName}";

            // Create HTML body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateOrderStatusUpdateHtml(orderDto)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Order status update email sent successfully to {Email} for order {OrderId} with status {Status}",
                orderDto.ShippingAddress.EmailAddress, orderDto.Id, orderDto.OrderStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order status update email for order {OrderId} to {Email}",
                orderDto.Id, orderDto.ShippingAddress.EmailAddress);
            return false;
        }
    }

    /// <summary>
    /// Generates HTML content for the order confirmation email.
    /// </summary>
    private static string GenerateOrderConfirmationHtml(OrderDto orderDto)
    {
        var itemsHtml = string.Join("", orderDto.OrderItems.Select(item =>
            $@"
                <tr>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{item.ProductId}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{item.Quantity}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{item.Price:C}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{(item.Quantity * item.Price):C}</td>
                </tr>"));

        var total = orderDto.OrderItems.Sum(item => item.Quantity * item.Price);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background-color: #4CAF50; color: white; padding: 10px; text-align: left; }}
        .total {{ font-size: 18px; font-weight: bold; text-align: right; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Confirmation de votre commande</h1>
        </div>
        <div class=""content"">
            <p>Bonjour {orderDto.ShippingAddress.FirstName} {orderDto.ShippingAddress.LastName},</p>
            <p>Nous vous confirmons la réception de votre commande <strong>{orderDto.OrderName}</strong>.</p>
            
            <h3>Détails de la commande :</h3>
            <table>
                <thead>
                    <tr>
                        <th>Produit ID</th>
                        <th>Quantité</th>
                        <th>Prix unitaire</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            
            <div class=""total"">
                Total : {total:C}
            </div>
            
            <h3>Adresse de livraison :</h3>
            <p>
                {orderDto.ShippingAddress.FirstName} {orderDto.ShippingAddress.LastName}<br>
                {orderDto.ShippingAddress.AddressLine}<br>
                {orderDto.ShippingAddress.ZipCode} {orderDto.ShippingAddress.State}<br>
                {orderDto.ShippingAddress.Country}
            </p>
            
            <h3>Statut de la commande :</h3>
            <p><strong>{GetOrderStatusInFrench(orderDto.OrderStatus)}</strong></p>
            
            <p>Nous vous tiendrons informé de l'avancement de votre commande.</p>
            <p>Cordialement,<br>L'équipe E-Shop</p>
        </div>
        <div class=""footer"">
            <p>Cet email est automatique, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Generates HTML content for the order status update email (cancellation, shipment, etc.).
    /// </summary>
    private static string GenerateOrderStatusUpdateHtml(OrderDto orderDto)
    {
        var itemsHtml = string.Join("", orderDto.OrderItems.Select(item =>
            $@"
                <tr>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{item.ProductId}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{item.Quantity}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{item.Price:C}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{(item.Quantity * item.Price):C}</td>
                </tr>"));

        var total = orderDto.OrderItems.Sum(item => item.Quantity * item.Price);
        var isCancelled = orderDto.OrderStatus == OrderStatus.Cancelled;
        var headerColor = isCancelled ? "#dc3545" : "#007bff"; // Red for cancellation, Blue for shipment
        var headerTitle = isCancelled ? "Commande annulée" : "Commande expédiée";
        var mainMessage = isCancelled
            ? $"Votre commande <strong>{orderDto.OrderName}</strong> a été annulée."
            : $"Votre commande <strong>{orderDto.OrderName}</strong> a été expédiée.";

        var additionalInfo = isCancelled
            ? "<p>Si vous avez des questions concernant cette annulation, n'hésitez pas à nous contacter.</p>"
            : "<p>Votre commande est en cours de livraison. Vous recevrez une notification lorsque votre colis sera livré.</p>";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {headerColor}; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background-color: {headerColor}; color: white; padding: 10px; text-align: left; }}
        .total {{ font-size: 18px; font-weight: bold; text-align: right; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .status-badge {{ display: inline-block; padding: 8px 16px; background-color: {headerColor}; color: white; border-radius: 4px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{headerTitle}</h1>
        </div>
        <div class=""content"">
            <p>Bonjour {orderDto.ShippingAddress.FirstName} {orderDto.ShippingAddress.LastName},</p>
            <p>{mainMessage}</p>
            
            <h3>Détails de la commande :</h3>
            <table>
                <thead>
                    <tr>
                        <th>Produit ID</th>
                        <th>Quantité</th>
                        <th>Prix unitaire</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            
            <div class=""total"">
                Total : {total:C}
            </div>
            
            <h3>Statut actuel :</h3>
            <p><span class=""status-badge"">{GetOrderStatusInFrench(orderDto.OrderStatus)}</span></p>
            
            {additionalInfo}
            
            <p>Cordialement,<br>L'équipe E-Shop</p>
        </div>
        <div class=""footer"">
            <p>Cet email est automatique, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Converts order status enum to French display name.
    /// </summary>
    private static string GetOrderStatusInFrench(OrderStatus orderStatus)
    {
        return orderStatus switch
        {
            OrderStatus.Draft => "Brouillon",
            OrderStatus.Pending => "En attente",
            OrderStatus.Submitted => "Soumis",
            OrderStatus.Cancelled => "Annulée",
            OrderStatus.Confirmed => "Confirmée",
            OrderStatus.Completed => "Terminée",
            OrderStatus.Shipped => "Expédiée",
            OrderStatus.Delivered => "Livrée",
            _ => "Inconnu"
        };
    }
}

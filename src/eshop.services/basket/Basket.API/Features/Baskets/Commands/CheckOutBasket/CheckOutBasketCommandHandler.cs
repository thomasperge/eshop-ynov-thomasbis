using Basket.API.Data.Repositories;
using Basket.API.Services;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Basket.API.Features.Baskets.Commands.CheckOutBasket;

/// <summary>
/// Handles the checkout process for a user's basket. This class retrieves the basket data,
/// validates stock availability, publishes a checkout event, and removes the basket from the repository after a successful checkout.
/// </summary>
/// <remarks>
/// This command handler processes <see cref="CheckOutBasketCommand"/> requests, executes the required business logic,
/// and returns a <see cref="CheckOutBasketCommandResult"/> indicating the outcome of the operation.
/// It also integrates with the messaging system via <see cref="IPublishEndpoint"/> to notify other systems
/// about the basket checkout event.
/// </remarks>
public class CheckOutBasketCommandHandler(
    IBasketRepository repository, 
    IPublishEndpoint publishEndpoint,
    ICatalogService catalogService,
    ILogger<CheckOutBasketCommandHandler> logger)
    : ICommandHandler<CheckOutBasketCommand, CheckOutBasketCommandResult>
{
    /// <summary>
    /// Handles the checkout process for the user's basket, validating stock before publishing a checkout event and deleting the basket.
    /// </summary>
    /// <param name="request">The command request containing the basket checkout details.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the checkout process.</returns>
    public async Task<CheckOutBasketCommandResult> Handle(CheckOutBasketCommand request,
        CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(request.BasketCheckoutDto.UserName, cancellationToken)
            .ConfigureAwait(false);
        
        if (basket == null || basket.Items == null || !basket.Items.Any())
        {
            logger.LogWarning("Basket not found or empty for user {UserName}", request.BasketCheckoutDto.UserName);
            return new CheckOutBasketCommandResult(false, "Le panier est vide ou introuvable.");
        }
        
        // Validate stock availability for each product in the basket
        var stockValidationFailures = new List<string>();
        
        foreach (var item in basket.Items)
        {
            var product = await catalogService.GetProductAsync(item.ProductId, cancellationToken);
            
            if (product == null)
            {
                stockValidationFailures.Add($"Produit {item.ProductId} introuvable dans le catalogue.");
                logger.LogWarning("Product {ProductId} not found in catalog during checkout", item.ProductId);
                continue;
            }
            
            if (product.Stock < item.Quantity)
            {
                stockValidationFailures.Add(
                    $"Produit {product.Name} (ID: {item.ProductId}): Stock insuffisant. Disponible: {product.Stock}, Demandé: {item.Quantity}");
                logger.LogWarning(
                    "Insufficient stock for product {ProductId} ({ProductName}). Available: {AvailableStock}, Requested: {Quantity}",
                    item.ProductId, product.Name, product.Stock, item.Quantity);
            }
        }
        
        // If any stock validation failed, return error without publishing event or deleting basket
        if (stockValidationFailures.Any())
        {
            var errorMessage = $"Stock insuffisant pour les produits suivants: {string.Join("; ", stockValidationFailures)}";
            logger.LogError("Checkout cancelled for user {UserName} due to stock validation failures: {ErrorMessage}",
                request.BasketCheckoutDto.UserName, errorMessage);
            return new CheckOutBasketCommandResult(false, errorMessage);
        }
        
        // Stock validation passed, proceed with checkout
        var eventMessage = request.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.Total;
        
        // Map basket items to event items
        eventMessage.Items = basket.Items.Select(item => new BuildingBlocks.Messaging.Events.BasketItemDto
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();
        
        await publishEndpoint.Publish(eventMessage, cancellationToken).ConfigureAwait(false);
        
        // Only delete basket after successful event publication
        await repository.DeleteBasketAsync(request.BasketCheckoutDto.UserName, cancellationToken).ConfigureAwait(false);
        
        logger.LogInformation("Checkout successful for user {UserName}. Basket deleted and event published.", 
            request.BasketCheckoutDto.UserName);
        
        return new CheckOutBasketCommandResult(true);
    }
}
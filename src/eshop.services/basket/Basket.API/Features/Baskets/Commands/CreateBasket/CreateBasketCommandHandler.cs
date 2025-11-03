using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using Discount.Grpc;
using Grpc.Core;

namespace Basket.API.Features.Baskets.Commands.CreateBasket;

/// <summary>
/// Handles the creation of a shopping basket by processing the CreateBasketCommand.
/// Implements the <see cref="ICommandHandler{CreateBasketCommand, CreateBasketCommandResult}"/> interface.
/// </summary>
public class CreateBasketCommandHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient, ILogger<CreateBasketCommandHandler> logger) : ICommandHandler<CreateBasketCommand, CreateBasketCommandResult>
{
    /// <summary>
    /// Handles the request to create a shopping basket.
    /// </summary>
    /// <param name="request">The CreateBasketCommand containing the details of the shopping basket to be created.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning a CreateBasketCommandResult that indicates the success of the operation and includes the UserName of the created basket.</returns>
    public async Task<CreateBasketCommandResult> Handle(CreateBasketCommand request,
        CancellationToken cancellationToken)
    {
        var cart = request.Cart;

        await ApplyDiscountToItemAsync(cart, cancellationToken);

        var basketCart = await repository.CreateBasketAsync(cart, cancellationToken)
            .ConfigureAwait(false);

        return new CreateBasketCommandResult(true, basketCart.UserName);
    }

    /// <summary>
    /// Applies a discount to each item in the specified shopping cart.
    /// Uses the CalculateDiscountedPrice gRPC method to handle Fixed and Percentage discounts properly.
    /// </summary>
    /// <param name="cart">The shopping cart containing the items to which the discount will be applied.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation of applying discounts to the items.</returns>
    private async Task ApplyDiscountToItemAsync(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            try
            {
                logger.LogInformation("Applying discount for product: {ProductName}", item.ProductName);
                
                // Get discount for this product
                var coupon = await discountProtoServiceClient.GetDiscountAsync(new GetDiscountRequest
                    { ProductName = item.ProductName }, cancellationToken: cancellationToken);
                
                logger.LogInformation("Discount retrieved - Amount: {Amount}, Percent: {Percent}", coupon.Amount, coupon.DiscountPercent);
                
                // If no discount found, GetDiscount returns Amount=0 AND DiscountPercent=0 (no exception)
                if (coupon.Amount == 0 && coupon.DiscountPercent == 0)
                {
                    logger.LogWarning("No discount found for product: {ProductName}", item.ProductName);
                    // No discount available, skip this item
                    continue;
                }
                
                // Use CalculateDiscountedPrice to handle both Fixed and Percentage discounts
                var calculateResponse = await discountProtoServiceClient.CalculateDiscountedPriceAsync(
                    new CalculatePriceRequest
                    {
                        OriginalPrice = (double)item.Price,
                        Coupon = coupon
                    }, 
                    cancellationToken: cancellationToken);
                
                logger.LogInformation("Calculated price: Original={OriginalPrice}, Discounted={DiscountedPrice}", item.Price, calculateResponse.DiscountedPrice);
                
                // Update item price with calculated discounted price
                item.Price = (decimal)calculateResponse.DiscountedPrice;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "Failed to apply discount for product: {ProductName}", item.ProductName);
                // Discount service unavailable - fallback: continue with original price
                // The item price remains unchanged (no discount applied)
            }
        }
    }
}
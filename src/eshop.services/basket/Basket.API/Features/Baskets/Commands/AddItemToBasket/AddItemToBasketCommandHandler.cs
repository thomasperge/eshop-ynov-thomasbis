using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using Discount.Grpc;
using Grpc.Core;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Gère l'ajout d'un article à un panier existant en traitant la commande AddItemToBasketCommand.
/// </summary>
public class AddItemToBasketCommandHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient, ILogger<AddItemToBasketCommandHandler> logger) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    /// <summary>
    /// Traite la demande d'ajout d'un article à un panier existant.
    /// </summary>
    /// <param name="request">La commande contenant les détails de l'article à ajouter.</param>
    /// <param name="cancellationToken">Un jeton pour observer l'attente de la fin de l'opération.</param>
    /// <returns>Un résultat indiquant le succès de l'opération et incluant le nom d'utilisateur du panier modifié.</returns>
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        // Appliquer le discount à l'article avant de l'ajouter
        await ApplyDiscountToItemAsync(request.item, cancellationToken);
        
        try
        {
            // Récupérer le panier existant
            var cart = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);
            
            // Ajouter l'article au panier (AddItem gère déjà l'incrémentation si existant)
            cart.AddItem(request.item);
            
            // Mettre à jour le panier
            await repository.UpdateBasketAsync(cart, cancellationToken);
            
            return new AddItemToBasketCommandResult(true, request.UserName);
        }
        catch
        {
            // Le panier n'existe pas, créer un nouveau panier avec cet article
            var cart = new ShoppingCart(request.UserName);
            cart.AddItem(request.item);
            await repository.CreateBasketAsync(cart, cancellationToken);
            
            return new AddItemToBasketCommandResult(true, request.UserName);
        }
    }
    
    /// <summary>
    /// Applique un discount à un article du panier.
    /// Utilise le service Discount pour calculer le prix réduit.
    /// </summary>
    /// <param name="item">L'article auquel appliquer le discount.</param>
    /// <param name="cancellationToken">Un jeton pour observer l'attente de la fin de l'opération.</param>
    /// <returns>Une tâche qui représente l'opération asynchrone d'application du discount.</returns>
    private async Task ApplyDiscountToItemAsync(ShoppingCartItem item, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Applying discount for product: {ProductName}", item.ProductName);
            
            // Obtenir le discount pour ce produit
            var coupon = await discountProtoServiceClient.GetDiscountAsync(new GetDiscountRequest
                { ProductName = item.ProductName }, cancellationToken: cancellationToken);
            
            logger.LogInformation("Discount retrieved - Amount: {Amount}, Percent: {Percent}", coupon.Amount, coupon.DiscountPercent);
            
            // Si aucun discount trouvé, GetDiscount retourne Amount=0 ET DiscountPercent=0 (pas d'exception)
            if (coupon.Amount == 0 && coupon.DiscountPercent == 0)
            {
                logger.LogWarning("No discount found for product: {ProductName}", item.ProductName);
                // Aucun discount disponible, ignorer cet article
                return;
            }
            
            // Utiliser CalculateDiscountedPrice pour gérer les discounts Fixe et Pourcentage
            var calculateResponse = await discountProtoServiceClient.CalculateDiscountedPriceAsync(
                new CalculatePriceRequest
                {
                    OriginalPrice = (double)item.Price,
                    Coupon = coupon
                }, 
                cancellationToken: cancellationToken);
            
            logger.LogInformation("Calculated price: Original={OriginalPrice}, Discounted={DiscountedPrice}", item.Price, calculateResponse.DiscountedPrice);
            
            // Mettre à jour le prix de l'article avec le prix réduit calculé
            item.Price = (decimal)calculateResponse.DiscountedPrice;
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "Failed to apply discount for product: {ProductName}", item.ProductName);
            // Service Discount indisponible - fallback: continuer avec le prix original
            // Le prix de l'article reste inchangé (aucun discount appliqué)
        }
    }
    
}

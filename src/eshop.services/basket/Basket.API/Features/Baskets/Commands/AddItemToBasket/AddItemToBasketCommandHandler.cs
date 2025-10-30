using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Gère l'ajout d'un article à un panier existant en traitant la commande AddItemToBasketCommand.
/// </summary>
public class AddItemToBasketCommandHandler(IBasketRepository repository) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    /// <summary>
    /// Traite la demande d'ajout d'un article à un panier existant.
    /// </summary>
    /// <param name="request">La commande contenant les détails de l'article à ajouter.</param>
    /// <param name="cancellationToken">Un jeton pour observer l'attente de la fin de l'opération.</param>
    /// <returns>Un résultat indiquant le succès de l'opération et incluant le nom d'utilisateur du panier modifié.</returns>
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        // Récupérer le panier existant
        var cart = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);
        
        if (cart == null)
        {
            // Si le panier n'existe pas, en créer un nouveau
            cart = new ShoppingCart(request.UserName);
        }
        
        // Vérifier si l'article existe déjà dans le panier
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.item.ProductId);
        
        if (existingItem != null)
        {
            // Mettre à jour la quantité si l'article existe déjà
            existingItem.Quantity += request.item.Quantity;
        }
        else
        {
            // Ajouter le nouvel article au panier
            cart.AddItem(request.item);
        }
        
        // Sauvegarder le panier mis à jour
        await repository.UpdateBasketAsync(cart, cancellationToken);
        
        return new AddItemToBasketCommandResult(true, request.UserName);
    }
    
}

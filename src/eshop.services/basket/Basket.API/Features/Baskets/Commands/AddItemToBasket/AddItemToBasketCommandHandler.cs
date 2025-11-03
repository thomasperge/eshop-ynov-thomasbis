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
    
}

using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Commande pour supprimer un article du panier.
/// </summary>
public record DeleteBasketItemCommand(string UserName, string ProductId) : ICommand<DeleteBasketItemCommandResult>;

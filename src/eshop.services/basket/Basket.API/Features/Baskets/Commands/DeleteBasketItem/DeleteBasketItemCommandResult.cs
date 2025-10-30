namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Résultat de la suppression d’un article du panier.
/// </summary>
public record DeleteBasketItemCommandResult(bool IsSuccess, string UserName);
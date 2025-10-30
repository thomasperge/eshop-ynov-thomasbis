namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Représente le résultat de l'exécution d'une commande pour ajouter un article à un panier.
/// </summary>
public record AddItemToBasketCommandResult(bool IsSuccess, string UserName);
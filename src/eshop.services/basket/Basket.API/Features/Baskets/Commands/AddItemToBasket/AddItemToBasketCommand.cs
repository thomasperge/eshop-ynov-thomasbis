using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Une commande pour ajouter un article à un panier existant.
/// </summary>
/// <param name="UserName">Nom d'utilisateur du propriétaire du panier.</param>
/// <param name="Item">L'article à ajouter au panier.</param>
public record AddItemToBasketCommand(string UserName, ShoppingCartItem item) : ICommand<AddItemToBasketCommandResult>;
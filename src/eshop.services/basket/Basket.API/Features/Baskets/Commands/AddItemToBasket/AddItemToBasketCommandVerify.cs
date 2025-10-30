using FluentValidation;
using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Validateur pour la classe <see cref="AddItemToBasketCommand"/> utilisé pour valider l'intégrité des données de la commande.
/// </summary>
public class AddItemToBasketCommandValidator : AbstractValidator<AddItemToBasketCommand>
{
    public AddItemToBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire");
        RuleFor(x => x.item.Quantity).GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");
        RuleFor(x => x.item.Color).NotEmpty().WithMessage("La couleur est obligatoire");
        RuleFor(x => x.item.ProductName).NotEmpty().WithMessage("Le nom est obligatoire");
    }
}
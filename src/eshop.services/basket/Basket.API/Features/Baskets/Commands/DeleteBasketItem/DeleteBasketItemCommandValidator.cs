using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Valide la commande de suppression d’un article du panier.
/// </summary>
public class DeleteBasketItemCommandValidator : AbstractValidator<DeleteBasketItemCommand>
{
    public DeleteBasketItemCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName est requis");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId est requis");
    }
}
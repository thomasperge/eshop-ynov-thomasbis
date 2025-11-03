using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Handler pour supprimer un article du panier.
/// </summary>
public class DeleteBasketItemCommandHandler(IBasketRepository repository) : ICommandHandler<DeleteBasketItemCommand, DeleteBasketItemCommandResult>
{
    public async Task<DeleteBasketItemCommandResult> Handle(DeleteBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);

            var initialCount = cart.Items.Count;
            var itemsToKeep = cart.Items.Where(i => i.ProductId.ToString() != request.ProductId).ToList();

            if (itemsToKeep.Count == initialCount)
            {
                return new DeleteBasketItemCommandResult(false, request.UserName);
            }

            cart.Items = itemsToKeep;
            await repository.UpdateBasketAsync(cart, cancellationToken);
            return new DeleteBasketItemCommandResult(true, request.UserName);
        }
        catch (Exception)
        {
            return new DeleteBasketItemCommandResult(false, request.UserName);
        }
    }
}
    

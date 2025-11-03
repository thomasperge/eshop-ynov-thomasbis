using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasketItemQuantity;

public class UpdateBasketItemQuantityCommandHandler(IBasketRepository repository)
    : ICommandHandler<UpdateBasketItemQuantityCommand, UpdateBasketItemQuantityCommandResult>
{
    public async Task<UpdateBasketItemQuantityCommandResult> Handle(UpdateBasketItemQuantityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var basket = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);

            var productGuid = Guid.Parse(request.ProductId);
            var item = basket.Items.FirstOrDefault(i => i.ProductId == productGuid);
            
            if (item == null)
                return new UpdateBasketItemQuantityCommandResult(false);

            item.Quantity = request.Quantity;
            await repository.UpdateBasketAsync(basket, cancellationToken);
            return new UpdateBasketItemQuantityCommandResult(true);
        }
        catch
        {
            return new UpdateBasketItemQuantityCommandResult(false);
        }
    }
}
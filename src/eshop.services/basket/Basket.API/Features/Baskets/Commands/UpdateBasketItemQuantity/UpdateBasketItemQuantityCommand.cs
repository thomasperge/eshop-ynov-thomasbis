using MediatR;

namespace Basket.API.Features.Baskets.Commands.UpdateBasketItemQuantity;

public record UpdateBasketItemQuantityCommand(string UserName, string ProductId, int Quantity)
    : IRequest<UpdateBasketItemQuantityCommandResult>;
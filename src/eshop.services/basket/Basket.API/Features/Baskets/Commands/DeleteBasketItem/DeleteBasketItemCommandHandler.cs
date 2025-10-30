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
            Console.WriteLine($"Panier trouvé avec {cart.Items.Count()} items");

            foreach (var item in cart.Items)
            {
                Console.WriteLine($"Item existant: {item.ProductId} - {item.ProductName}");
            }
            Console.WriteLine($"ProductId recherché: {request.ProductId}");

            // Convertir le string en Guid pour la comparaison
            
            var initialCount = cart.Items.Count();
Console.WriteLine($"Format item.ProductId: '{cart.Items.First().ProductId.ToString()}'");
            Console.WriteLine($"Format request.ProductId: '{request.ProductId}'");
            
            var itemsToKeep = cart.Items.Where(i => i.ProductId.ToString() != request.ProductId).ToList();

            if (itemsToKeep.Count == initialCount)
            {
                Console.WriteLine("Aucun item supprimé - item non trouvé");
                return new DeleteBasketItemCommandResult(false, request.UserName);
            }


            cart.Items = itemsToKeep;
            await repository.CreateBasketAsync(cart, cancellationToken);
            Console.WriteLine($"Item supprimé avec succès. Reste {cart.Items.Count()} items");
            return new DeleteBasketItemCommandResult(true, request.UserName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return new DeleteBasketItemCommandResult(false, request.UserName);
        }
    }


    }

    
    

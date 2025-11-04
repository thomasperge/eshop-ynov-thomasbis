using Basket.API.Features.Baskets.Commands.CheckOutBasket;
using Basket.API.Features.Baskets.Commands.CreateBasket;
using Basket.API.Features.Baskets.Commands.AddItemToBasket;
using Basket.API.Features.Baskets.Commands.DeleteBasket;
using Basket.API.Features.Baskets.Queries.GetBasketByUserName;
using Basket.API.Features.Baskets.Commands.UpdateBasketItemQuantity;
using Basket.API.Features.Baskets.Commands.DeleteBasketItem;

using Basket.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

/// <summary>
/// The BasketsController is responsible for handling HTTP requests related to user baskets in the basket service.
/// It provides endpoints to retrieve the shopping basket for a specific user.
/// </summary>
[ApiController]
[Route("[controller]/{userName}")]
[Produces("application/json")]
public class BasketsController (ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves the shopping basket for the specified user.
    /// </summary>
    /// <param name="userName">The username whose shopping basket is to be retrieved.</param>
    /// <returns>The shopping basket associated with the specified username or a not-found response if no basket exists.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCart>> GetBasketByUserName(string userName)
    {
        var result = await sender.Send(new GetBasketByUserNameQuery(userName));
        return Ok(result.ShoppingCart);
    }

    /// <summary>
    /// Creates a shopping basket for the specified user based on the given request data.
    /// </summary>
    /// <param name="userName">The username for whom the shopping basket is to be created.</param>
    /// <param name="request">The request containing the details of the shopping basket to be created.</param>
    /// <returns>The result of the create basket operation, including success status and associated username.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBasketCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBasketCommandResult>> CreateBasket(string userName, [FromBody] CreateBasketCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetBasketByUserName), new { userName }, result);
    }
    
    
    /// <summary>
    /// Supprime le panier complet de l'utilisateur spécifié.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont le panier doit être supprimé.</param>
    /// <returns>Le résultat de l'opération de suppression.</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(DeleteBasketCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteBasketCommandResult>> DeleteBasket(string userName)
    {
        var result = await sender.Send(new DeleteBasketCommand(userName));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    
    /// <summary>
    /// Supprime un article du panier de l'utilisateur spécifié.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont l'article doit être supprimé du panier.</param>
    /// <param name="productId">L'identifiant du produit à supprimer.</param>
    /// <returns>Le résultat de l'opération de suppression.</returns>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(DeleteBasketItemCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteBasketItemCommandResult>> DeleteBasketItem(string userName, string productId)
    {
        var result = await sender.Send(new DeleteBasketItemCommand(userName, productId));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    
    // TODO Update basket product quantity
    
    //TODO Delete item in user basket
    
    /// <summary>
    /// Processes the checkout operation for the specified user's basket.
    /// </summary>
    /// <param name="userName">The username whose basket is to be checked out.</param>
    /// <param name="request">The details of the checkout request, including basket information.</param>
    /// <returns>The result of the checkout operation, indicating success or failure status.</returns>
    [HttpPost("Checkout")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    public async Task<ActionResult<bool>> CheckOutBasket(string userName, [FromBody] CheckOutBasketCommand request)
    {
        request.BasketCheckoutDto.UserName = userName;
        var result = await sender.Send(request);
        return Ok(result.IsSuccess);
    }
}
using Basket.API.Features.Baskets.Commands.CreateBasket;
using Basket.API.Features.Baskets.Commands.AddItemToBasket;
using Basket.API.Features.Baskets.Commands.DeleteBasket;
using Basket.API.Features.Baskets.Queries.GetBasketByUserName;
using Basket.API.Features.Baskets.Commands.UpdateBasketItemQuantity;
using Basket.API.Features.Baskets.Commands.DeleteBasketItem;
using Basket.API.Features.Baskets.Commands.CheckOutBasket;
using Basket.API.Features.Baskets.Dtos;

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



    /// <summary>
    /// Ajoute un article au panier de l'utilisateur spécifié.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont le panier doit être mis à jour.</param>
    /// <param name="item">L'article à ajouter au panier.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la requête.</param>
    /// <returns>Le résultat de l'opération d'ajout.</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(AddItemToBasketCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddItemToBasketCommandResult>> AddItemToBasket(
        [FromRoute] string userName,
        [FromBody] ShoppingCartItem item,
        CancellationToken cancellationToken)
    {
        var command = new AddItemToBasketCommand(userName, item);
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Met à jour la quantité d'un produit dans le panier de l'utilisateur spécifié.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont l'article doit être mis à jour.</param>
    /// <param name="productId">L'identifiant du produit à mettre à jour.</param>
    /// <param name="quantity">La nouvelle quantité.</param>
    /// <returns>Le résultat de l'opération de mise à jour.</returns>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(UpdateBasketItemQuantityCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UpdateBasketItemQuantityCommandResult>> UpdateBasketItemQuantity(
        [FromRoute] string userName,
        [FromRoute] string productId,
        [FromBody] int quantity)
    {
        if (quantity <= 0)
            return BadRequest("La quantité doit être supérieure à 0.");
            
        var command = new UpdateBasketItemQuantityCommand(userName, productId, quantity);
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    
    /// <summary>
    /// Met à jour complètement le panier de l'utilisateur spécifié.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont le panier doit être mis à jour.</param>
    /// <param name="cart">Le panier complet avec les articles mis à jour.</param>
    /// <returns>Le résultat de l'opération de mise à jour.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateBasket(string userName, [FromBody] ShoppingCart cart)
    {
        if (cart == null || cart.Items == null || !cart.Items.Any())
            return BadRequest("Le panier est vide ou invalide.");
        
        if (!string.Equals(userName, cart.UserName, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Le nom d'utilisateur du panier ne correspond pas à celui de la route.");
        
        var result = await sender.Send(new CreateBasketCommand(cart)); 

        if (result == null)
            return NotFound($"Aucun panier trouvé pour l'utilisateur : '{userName}'.");

        return Ok(true);
    }

    /// <summary>
    /// Effectue le checkout du panier de l'utilisateur spécifié.
    /// Publie un événement BasketCheckoutEvent et supprime le panier après succès.
    /// </summary>
    /// <param name="userName">Le nom d'utilisateur dont le panier doit être checkout.</param>
    /// <param name="checkoutDto">Les détails du checkout (adresse, paiement, etc.).</param>
    /// <returns>Le résultat de l'opération de checkout.</returns>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckOutBasketCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckOutBasketCommandResult>> CheckOutBasket(
        [FromRoute] string userName,
        [FromBody] BasketCheckoutDto checkoutDto)
    {
        if (!string.Equals(userName, checkoutDto.UserName, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Le nom d'utilisateur du checkout ne correspond pas à celui de la route.");
        
        var command = new CheckOutBasketCommand(checkoutDto);
        var result = await sender.Send(command);
        return Ok(result);
    }
}
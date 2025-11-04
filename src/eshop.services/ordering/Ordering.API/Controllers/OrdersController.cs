using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Features.Orders.Queries.GetOrders;
using Ordering.Application.Features.Orders.Queries.GetOrdersByName;
using Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;
using Ordering.Application.Features.Orders.Queries.GetOrderById;
using Ordering.Domain.Enums;

namespace Ordering.API.Controllers;

/// <summary>
/// API controller responsible for managing and accessing order data.
/// Provides endpoints for retrieving, creating, and deleting orders associated with customers or specific order names.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class OrdersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of orders filtered by the provided order name.
    /// </summary>
    /// <param name="name">The name used to filter the orders.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects that match the specified name.</returns>
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByName(string name)
    {
        var result = await sender.Send(new GetOrdersByNameQuery(name));
        if (!result.Orders.Any())
        {
            return NotFound($"No orders found with name containing '{name}'");
        }
        return Ok(result.Orders);
    }

    /// <summary>
    /// Retrieves a list of orders associated with a specific customer identified by their unique ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders are being retrieved.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects associated with the specified customer.</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomerId(Guid customerId)
    {
        var result = await sender.Send(new GetOrdersByCustomerIdQuery(customerId));
        if (!result.Orders.Any())
        {
            return NotFound($"No orders found for customer with ID '{customerId}'");
        }
        return Ok(result.Orders);
    }


    /// <summary>
    /// Retrieves a paginated list of orders based on the specified page index and page size.
    /// </summary>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of orders to include in each page of results.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects representing the paginated list of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrdersQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetOrdersQueryResult>> GetOrders([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        if (pageIndex < 0)
        {
            return BadRequest("PageIndex must be greater than or equal to 0");
        }
        if (pageSize <= 0)
    {
            return BadRequest("PageSize must be greater than 0");
        }

        var result = await sender.Send(new GetOrdersQuery(pageIndex, pageSize));
        if (!result.Orders.Any())
        {
            return NotFound("No orders found");
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order based on the provided order details.
    /// </summary>
    /// <param name="order">The <see cref="OrderDto"/> containing details of the order to be created.</param>
    /// <returns>The unique identifier of the newly created order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] OrderDto order)
    {
        var result = await sender.Send(new CreateOrderCommand(order));
        return Ok(result.NewOrderId);
    }

    /// <summary>
    /// Retrieves a single order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>The <see cref="OrderDto"/> object representing the requested order.</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId)
    {
        var result = await sender.Send(new GetOrderByIdQuery(orderId));
        if (result.Order == null)
        {
            return NotFound($"Order with ID '{orderId}' not found");
        }
        return Ok(result.Order);
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to update.</param>
    /// <param name="orderStatus">The new status to set for the order.</param>
    /// <returns>A boolean indicating whether the status update was successful.</returns>
    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatus orderStatus)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(orderId, orderStatus));
        return Ok(result.IsSuccess);
    }

    /// <summary>
    /// Updates an existing order with the provided order details.
    /// </summary>
    /// <param name="order">The updated order details encapsulated in an <see cref="OrderDto"/>.</param>
    /// <returns>A boolean result indicating whether the update operation was successful.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateOrder([FromBody] OrderDto order)
    {
        var result = await sender.Send(new UpdateOrderCommand(order));
        return Ok(result.IsSuccess);
    }

    /// <summary>
    /// Deletes an order based on the provided order identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteOrder(Guid orderId)
    {
        var result = await sender.Send(new DeleteOrderCommand(orderId));
        return Ok(result.IsSuccess);
    }
}
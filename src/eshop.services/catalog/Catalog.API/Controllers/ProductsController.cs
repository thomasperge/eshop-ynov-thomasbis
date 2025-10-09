using Catalog.API.Features.Products.Commands.CreateProduct;
using Catalog.API.Features.Products.Commands.DeleteProduct;
using Catalog.API.Features.Products.Commands.ImportProducts;
using Catalog.API.Features.Products.Commands.UpdateProduct;
using Catalog.API.Features.Products.Queries.GetProductById;
using Catalog.API.Features.Products.Queries.ReadProducts;
using Catalog.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

/// <summary>
/// Manages operations related to products within the catalog, including retrieving product data
/// and creating new products.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(Guid id)
    {
        var result = await sender.Send(new GetProductByIdQuery(id));
        return Ok(result.Product);
    }

    /// <summary>
    /// Retrieves products filtered by category with pagination.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ReadProductsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadProductsQueryResult>> GetProductsByCategory(
        string category,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Category is required");

        var result = await sender.Send(new ReadProductsQuery(pageNumber, pageSize, category));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all products with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReadProductsQueryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReadProductsQueryResult>> GetProducts(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize)
    {
        var result = await sender.Send(new ReadProductsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateProductCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateProductCommandResult>> CreateProduct(CreateProductCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand request)
    {
        var result = await sender.Send(request);
        return Ok(result.IsSuccessful);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteProduct(Guid id)
    {
        var result = await sender.Send(new DeleteProductCommand(id));
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Imports products from an Excel file.
    /// </summary>
    /// <param name="excelFile">The Excel file containing the products to import.</param>
    /// <returns>A result object containing the import statistics.</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportProductsCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportProductsCommandResult>> ImportProducts(IFormFile excelFile)
    {
        var command = new ImportProductsCommand(excelFile);
        var result = await sender.Send(command);
        
        if (result.FailedImports > 0 && result.SuccessfullyImported == 0)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
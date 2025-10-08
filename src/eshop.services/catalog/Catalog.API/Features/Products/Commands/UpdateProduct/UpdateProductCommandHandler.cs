using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handles the UpdateProduct command to update an existing product in the system.
/// </summary>
public class UpdateProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the UpdateProduct command, which updates an existing product in the system.
    /// </summary>
    /// <param name="request">The UpdateProduct command containing the product details to update.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the result indicating success.</returns>
    /// <exception cref="NotFoundException">Thrown when the product with the specified ID does not exist.</exception>
    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await documentSession.Query<Product>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);

        // Update product properties
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.ImageFile = request.ImageFile;
        product.Categories = request.Categories;

        documentSession.Update(product);
        await documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResult(true);
    }
}
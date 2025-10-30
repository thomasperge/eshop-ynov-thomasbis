using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles the DeleteProduct command to delete an existing product from the system by removing it through the provided document session.
/// </summary>
public class DeleteProductCommandHandler(IDocumentSession documentSession): ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the DeleteProduct command, which removes a product from the system.
    /// It ensures the product exists before attempting to delete it.
    /// </summary>
    /// <param name="request">The DeleteProduct command containing the ID of the product to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the result of the command which indicates success.</returns>
    /// <exception cref="NotFoundException">Thrown when the product with the specified ID does not exist in the system.</exception>
    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await documentSession.Query<Product>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);
        
        documentSession.Delete(product);
        
        await documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteProductCommandResult(true);
    }
}


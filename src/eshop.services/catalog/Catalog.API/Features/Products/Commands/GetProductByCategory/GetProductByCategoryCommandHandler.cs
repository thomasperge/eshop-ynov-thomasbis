using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.GetProductByCategory;

/// <summary>
/// Handles retrieving products filtered by category.
/// </summary>
public class GetProductByCategoryCommandHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductByCategoryCommand, GetProductByCategoryCommandResult>
{
    public async Task<GetProductByCategoryCommandResult> Handle(GetProductByCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedCategory = request.Category.Trim().ToLower();

        var products = await documentSession.Query<Product>()
            .Where(p => p.Categories.Any(c => c.ToLower() == normalizedCategory))
            .ToListAsync(cancellationToken);

        return new GetProductByCategoryCommandResult(products);
    }
}
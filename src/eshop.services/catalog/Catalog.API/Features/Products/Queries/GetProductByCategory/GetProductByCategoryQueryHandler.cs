using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Handles retrieving products filtered by category.
/// </summary>
public class GetProductByCategoryQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryQueryResult>
{
    public async Task<GetProductByCategoryQueryResult> Handle(GetProductByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Where(p => p.Categories.Contains(request.Category))
            .ToListAsync(cancellationToken);

        return new GetProductByCategoryQueryResult(products);
    }
}
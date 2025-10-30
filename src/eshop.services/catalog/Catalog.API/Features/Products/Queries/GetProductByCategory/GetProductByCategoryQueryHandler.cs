using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Handles retrieving products filtered by category (case-insensitive).
/// </summary>
public class GetProductByCategoryQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryQueryResult>
{
    public async Task<GetProductByCategoryQueryResult> Handle(GetProductByCategoryQuery request, CancellationToken cancellationToken)
    {
        var normalizedCategory = request.Category.Trim().ToLower();

        var allProducts = await documentSession.Query<Product>()
            .ToListAsync(cancellationToken);

        var filtered = allProducts
            .Where(p => p.Categories
                .Any(c => string.Equals(c, normalizedCategory, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new GetProductByCategoryQueryResult(filtered);
    }
}
using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using Marten.Linq;

namespace Catalog.API.Features.Products.Queries.SearchProducts;

/// <summary>
/// Handles the SearchProducts query to search and filter products dynamically.
/// </summary>
public class SearchProductsQueryHandler(IDocumentSession documentSession) : IQueryHandler<SearchProductsQuery, SearchProductsQueryResult>
{
    /// <summary>
    /// Handles the processing of the SearchProducts query, applying dynamic filters and pagination.
    /// </summary>
    /// <param name="request">The SearchProducts query containing the search criteria.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the filtered and paginated products.</returns>
    public async Task<SearchProductsQueryResult> Handle(SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Commencer avec tous les produits
        IMartenQueryable<Product> query = documentSession.Query<Product>();

        // Filtre 1 : Recherche par terme (nom ou description)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = (IMartenQueryable<Product>)query.Where(p =>
                p.Name.ToLower().Contains(searchTermLower) ||
                p.Description.ToLower().Contains(searchTermLower));
        }

        // Filtre 2 : Recherche par catégories
        if (request.Categories != null && request.Categories.Any())
        {
            // Filtrer les produits qui ont au moins une catégorie de la liste
            query = (IMartenQueryable<Product>)query.Where(p => p.Categories.Any(c => request.Categories.Contains(c)));
        }

        // Filtre 3 : Prix minimum
        if (request.MinPrice.HasValue)
        {
            query = (IMartenQueryable<Product>)query.Where(p => p.Price >= request.MinPrice.Value);
        }

        // Filtre 4 : Prix maximum
        if (request.MaxPrice.HasValue)
        {
            query = (IMartenQueryable<Product>)query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        // Appliquer le tri
        IQueryable<Product> orderedQuery = request.SortBy.ToLower() switch
        {
            "price" => request.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "date" => request.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id),
            _ => request.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };

        // Compter le total avant pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculer le nombre total de pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Appliquer la pagination
        var skip = (request.PageNumber - 1) * request.PageSize;
        var productsResult = await orderedQuery
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Convertir IReadOnlyList en List
        var products = productsResult.ToList();

        return new SearchProductsQueryResult(
            products,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}


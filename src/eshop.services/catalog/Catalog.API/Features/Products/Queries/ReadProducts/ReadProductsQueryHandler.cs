using Catalog.API.Models;
using MediatR;
using Marten;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.API.Features.Products.Queries.ReadProducts;

public class ReadProductsQueryHandler : IRequestHandler<ReadProductsQuery, ReadProductsQueryResult>
{
    private readonly IDocumentSession _session;

    public ReadProductsQueryHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<ReadProductsQueryResult> Handle(ReadProductsQuery request, CancellationToken cancellationToken)
    {
        // ✅ Utilise IQueryable au lieu de IMartenQueryable
        IQueryable<Product> query = _session.Query<Product>();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Categories.Contains(request.Category));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        // ✅ Marten retourne un IReadOnlyList, donc convertis en List
        var products = (await query
                .OrderBy(p => p.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken))
            .ToList(); // ← ici

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        return new ReadProductsQueryResult(
            request.PageNumber,
            request.PageSize,
            totalItems,
            totalPages,
            products
        );
    }
}
using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ReadProducts;

/// <summary>
/// Requête pour récupérer une liste paginée de produits, éventuellement filtrée par catégorie.
/// </summary>
/// <param name="PageNumber">Le numéro de la page à récupérer (commence à 1).</param>
/// <param name="PageSize">Le nombre d'éléments par page.</param>
/// <param name="Category">Catégorie facultative pour filtrer les résultats.</param>
public record ReadProductsQuery(
    int PageNumber,
    int PageSize,
    string? Category = null
) : IQuery<ReadProductsQueryResult>;
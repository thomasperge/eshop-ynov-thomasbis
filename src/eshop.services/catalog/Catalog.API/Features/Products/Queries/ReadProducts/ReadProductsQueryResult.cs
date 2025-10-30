using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.ReadProducts;

/// <summary>
/// Représente le résultat d'une requête paginée pour lire les produits.
/// </summary>
/// <param name="Page">Numéro de la page actuelle (commence à 1).</param>
/// <param name="Size">Nombre d'éléments par page.</param>
/// <param name="TotalItems">Nombre total d'éléments disponibles.</param>
/// <param name="TotalPages">Nombre total de pages calculé.</param>
/// <param name="Products">Liste des produits retournés pour la page courante.</param>
public record ReadProductsQueryResult(
    int Page,
    int Size,
    int TotalItems,
    int TotalPages,
    List<Product> Products
);
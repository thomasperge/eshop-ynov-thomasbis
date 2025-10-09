using FluentValidation;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Validates the ExportProductsQuery to ensure that all required conditions are met.
/// </summary>
/// <remarks>
/// Currently no specific validation is required for this query as it has no parameters.
/// This validator is included for consistency and future extensibility.
/// </remarks>
public class ExportProductsQueryValidator : AbstractValidator<ExportProductsQuery>
{
    /// <summary>
    /// Provides validation rules for the ExportProductsQuery.
    /// </summary>
    public ExportProductsQueryValidator()
    {
        // Pas de validation nécessaire pour cette query
        // Le validator est créé pour maintenir la cohérence avec les autres features
    }
}


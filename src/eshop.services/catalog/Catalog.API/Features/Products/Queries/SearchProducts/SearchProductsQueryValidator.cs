using FluentValidation;

namespace Catalog.API.Features.Products.Queries.SearchProducts;

/// <summary>
/// Validates the SearchProductsQuery to ensure that all search parameters meet the defined constraints.
/// </summary>
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    /// <summary>
    /// Provides validation rules for the SearchProductsQuery.
    /// </summary>
    public SearchProductsQueryValidator()
    {
        // Validation de la pagination
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        // Validation des prix
        RuleFor(query => query.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MinPrice.HasValue)
            .WithMessage("Minimum price must be greater than or equal to 0");

        RuleFor(query => query.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MaxPrice.HasValue)
            .WithMessage("Maximum price must be greater than or equal to 0");

        // Validation que MaxPrice >= MinPrice
        RuleFor(query => query.MaxPrice)
            .GreaterThanOrEqualTo(query => query.MinPrice!.Value)
            .When(query => query.MinPrice.HasValue && query.MaxPrice.HasValue)
            .WithMessage("Maximum price must be greater than or equal to minimum price");

        // Validation du champ de tri
        RuleFor(query => query.SortBy)
            .Must(sortBy => new[] { "name", "price", "date" }.Contains(sortBy.ToLower()))
            .WithMessage("SortBy must be one of: Name, Price, Date");

        // Validation de l'ordre de tri
        RuleFor(query => query.SortOrder)
            .Must(sortOrder => new[] { "asc", "desc" }.Contains(sortOrder.ToLower()))
            .WithMessage("SortOrder must be either Asc or Desc");
    }
}


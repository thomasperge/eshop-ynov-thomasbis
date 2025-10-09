using FluentValidation;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Validates the GetProductByCategoryQuery.
/// </summary>
public class GetProductByCategoryQueryValidator : AbstractValidator<GetProductByCategoryQuery>
{
    public GetProductByCategoryQueryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MinimumLength(2).WithMessage("Category must have at least 2 characters");
    }
}
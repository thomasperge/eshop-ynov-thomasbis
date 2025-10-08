using FluentValidation;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Validates the UpdateProductCommand to ensure all required properties meet the defined rules.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Provides validation rules for the UpdateProductCommand.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(product => product.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(product => product.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(product => product.Categories).NotEmpty().WithMessage("Categories are required");
        RuleFor(product => product.ImageFile).NotEmpty().WithMessage("ImageFile is required");
        RuleFor(product => product.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(product => product.Price).GreaterThanOrEqualTo(1).WithMessage("Price must be greater than or equal to 1");
    }
}
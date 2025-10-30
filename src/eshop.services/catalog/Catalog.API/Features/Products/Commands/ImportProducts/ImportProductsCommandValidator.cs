using FluentValidation;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Validates the ImportProductsCommand to ensure that the Excel file meets the required criteria.
/// </summary>
public class ImportProductsCommandValidator : AbstractValidator<ImportProductsCommand>
{
    /// <summary>
    /// Provides validation rules for the ImportProductsCommand.
    /// </summary>
    public ImportProductsCommandValidator()
    {
        RuleFor(command => command.ExcelFile)
            .NotNull()
            .WithMessage("Excel file is required");

        RuleFor(command => command.ExcelFile.Length)
            .GreaterThan(0)
            .When(command => command.ExcelFile != null)
            .WithMessage("Excel file cannot be empty");

        RuleFor(command => command.ExcelFile.ContentType)
            .Must(contentType => contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ||
                                contentType == "application/vnd.ms-excel")
            .When(command => command.ExcelFile != null)
            .WithMessage("File must be a valid Excel file (.xlsx or .xls)");

        RuleFor(command => command.ExcelFile.FileName)
            .Must(fileName => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                             fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            .When(command => command.ExcelFile != null)
            .WithMessage("File extension must be .xlsx or .xls");
    }
}

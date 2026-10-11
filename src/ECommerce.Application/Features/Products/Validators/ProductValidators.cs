using ECommerce.Application.Features.Products.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantValidator());
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");
    }
}

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantDto>
{
    public CreateProductVariantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Variant name is required.")
            .MaximumLength(200).WithMessage("Variant name cannot exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Variant price must be greater than zero.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}

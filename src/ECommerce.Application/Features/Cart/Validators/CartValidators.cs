using ECommerce.Application.Features.Cart.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Cart.Validators;

public class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ProductVariantId != Guid.Empty || (x.VariantId.HasValue && x.VariantId.Value != Guid.Empty) || (x.ProductId.HasValue && x.ProductId.Value != Guid.Empty))
            .WithMessage("Product or variant is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

using ECommerce.Application.Features.StockMovements.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.StockMovements.Validators;

public class CreateStockAdjustmentValidator : AbstractValidator<CreateStockAdjustmentDto>
{
    public CreateStockAdjustmentValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Quantity).NotEqual(0).WithMessage("Adjustment quantity cannot be zero.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

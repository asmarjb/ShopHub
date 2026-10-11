using ECommerce.Application.Features.Coupons.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Coupons.Validators;

public class CreateCouponValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");
    }
}

public class ApplyCouponValidator : AbstractValidator<ApplyCouponDto>
{
    public ApplyCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.");

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage("SubTotal must be greater than zero.");
    }
}

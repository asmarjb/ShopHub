using ECommerce.Application.Features.Shipping.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Shipping.Validators;

public class CreateShippingMethodValidator : AbstractValidator<CreateShippingMethodDto>
{
    public CreateShippingMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.EstimatedDeliveryDays)
            .NotEmpty().WithMessage("Estimated delivery days is required.")
            .MaximumLength(100).WithMessage("Estimated delivery days cannot exceed 100 characters.");
    }
}

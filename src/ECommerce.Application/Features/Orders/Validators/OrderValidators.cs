using ECommerce.Application.Features.Orders.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Orders.Validators;

public class CheckoutValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.ShippingFullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.ShippingPhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(30);

        RuleFor(x => x.ShippingAddressLine1)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(250);

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.ShippingState)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100);

        RuleFor(x => x.ShippingPostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(30);

        RuleFor(x => x.ShippingCountry)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100);

        RuleFor(x => x.ShippingMethodId)
            .NotEmpty().WithMessage("Shipping method must be selected.");
    }
}

public class ChangeOrderStatusValidator : AbstractValidator<ChangeOrderStatusDto>
{
    public ChangeOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}

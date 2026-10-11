using ECommerce.Application.Features.Returns.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Returns.Validators;

public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequestDto>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerNote).MaximumLength(1000);
    }
}

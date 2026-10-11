using ECommerce.Application.Features.Brands.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Brands.Validators;

public class CreateBrandValidator : AbstractValidator<CreateBrandDto>
{
    public CreateBrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(150).WithMessage("Brand name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("LogoUrl cannot exceed 500 characters.");
    }
}

public class UpdateBrandValidator : AbstractValidator<UpdateBrandDto>
{
    public UpdateBrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(150).WithMessage("Brand name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("LogoUrl cannot exceed 500 characters.");
    }
}

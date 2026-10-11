using ECommerce.Application.Features.Categories.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Categories.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters.");
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters.");
    }
}

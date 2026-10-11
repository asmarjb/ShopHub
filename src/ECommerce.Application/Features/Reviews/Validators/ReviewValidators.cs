using ECommerce.Application.Features.Reviews.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Reviews.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Verified purchase order reference is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Review title is required.")
            .MaximumLength(150);

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(2000);
    }
}

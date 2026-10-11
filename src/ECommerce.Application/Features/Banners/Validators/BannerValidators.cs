using ECommerce.Application.Features.Banners.DTOs;
using FluentValidation;

namespace ECommerce.Application.Features.Banners.Validators;

public class CreateBannerValidator : AbstractValidator<CreateBannerDto>
{
    public CreateBannerValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(500);
    }
}

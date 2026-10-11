namespace ECommerce.Application.Features.Brands.DTOs;

public record BrandDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    bool IsActive);

public record CreateBrandDto(
    string Name,
    string? Description,
    string? LogoUrl,
    bool IsActive = true);

public record UpdateBrandDto(
    string Name,
    string? Description,
    string? LogoUrl,
    bool IsActive);

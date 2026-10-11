namespace ECommerce.Application.Features.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    int DisplayOrder,
    bool IsActive,
    bool IsDeleted,
    DateTime? DeletedAt,
    string? DeletedBy);

public record CategoryTreeDto(
    Guid Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int DisplayOrder,
    List<CategoryTreeDto> SubCategories);

public record CreateCategoryDto(
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    int DisplayOrder,
    bool IsActive = true);

public record UpdateCategoryDto(
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    int DisplayOrder,
    bool IsActive);

public record CategoryDeleteImpactDto(
    Guid CategoryId,
    string CategoryName,
    int ActiveProductsCount,
    int ChildCategoriesCount,
    int TotalProductsCount);


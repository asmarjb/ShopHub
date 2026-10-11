using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Categories.DTOs;

namespace ECommerce.Application.Features.Categories.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<CategoryDeleteImpactDto>> GetDeleteImpactAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteAsync(Guid id, Guid? reassignToCategoryId = null, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> HardDeleteAsync(Guid id, bool confirm, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetDeletedCategoriesAsync(CancellationToken cancellationToken = default);
}

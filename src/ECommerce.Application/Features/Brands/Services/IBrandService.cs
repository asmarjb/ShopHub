using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Brands.DTOs;

namespace ECommerce.Application.Features.Brands.Services;

public interface IBrandService
{
    Task<Result<IReadOnlyList<BrandDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<BrandDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);
    Task<Result<BrandDto>> UpdateAsync(Guid id, UpdateBrandDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

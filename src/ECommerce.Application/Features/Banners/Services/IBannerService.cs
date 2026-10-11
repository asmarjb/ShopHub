using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Banners.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Banners.Services;

public interface IBannerService
{
    Task<Result<IReadOnlyList<BannerDto>>> GetActiveBannersAsync(BannerPosition? position = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<BannerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<BannerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<BannerDto>> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken = default);
    Task<Result<BannerDto>> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

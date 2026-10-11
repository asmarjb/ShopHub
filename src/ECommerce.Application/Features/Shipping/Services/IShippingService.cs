using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Shipping.DTOs;

namespace ECommerce.Application.Features.Shipping.Services;

public interface IShippingService
{
    Task<Result<IReadOnlyList<ShippingMethodDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<ShippingMethodDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ShippingMethodDto>> CreateAsync(CreateShippingMethodDto dto, CancellationToken cancellationToken = default);
    Task<Result<ShippingMethodDto>> UpdateAsync(Guid id, UpdateShippingMethodDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

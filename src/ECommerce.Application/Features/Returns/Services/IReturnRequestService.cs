using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Returns.DTOs;

namespace ECommerce.Application.Features.Returns.Services;

public interface IReturnRequestService
{
    Task<Result<PagedResult<ReturnRequestDto>>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ReturnRequestDto>>> GetUserReturnRequestsAsync(CancellationToken cancellationToken = default);
    Task<Result<ReturnRequestDto>> CreateAsync(CreateReturnRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result<ReturnRequestDto>> ProcessAsync(Guid id, ProcessReturnRequestDto dto, CancellationToken cancellationToken = default);
}

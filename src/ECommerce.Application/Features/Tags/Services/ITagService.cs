using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Tags.DTOs;

namespace ECommerce.Application.Features.Tags.Services;

public interface ITagService
{
    Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default);
    Task<Result<TagDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TagDto>> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

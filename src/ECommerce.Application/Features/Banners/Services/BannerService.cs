using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Banners.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Banners.Services;

public class BannerService : IBannerService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BannerService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<BannerDto>>> GetActiveBannersAsync(BannerPosition? position = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.Banners
            .AsNoTracking()
            .Where(b => b.IsActive
                     && (!b.StartDate.HasValue || b.StartDate.Value <= now)
                     && (!b.EndDate.HasValue || b.EndDate.Value >= now));

        if (position.HasValue)
            query = query.Where(b => b.Position == position.Value);

        var banners = await query.OrderBy(b => b.DisplayOrder).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<BannerDto>>(_mapper.Map<List<BannerDto>>(banners));
    }

    public async Task<Result<IReadOnlyList<BannerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var banners = await _context.Banners
            .AsNoTracking()
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BannerDto>>(_mapper.Map<List<BannerDto>>(banners));
    }

    public async Task<Result<BannerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var banner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (banner == null)
            return Result.Failure<BannerDto>(Error.NotFound("Banner", id));

        return Result.Success(_mapper.Map<BannerDto>(banner));
    }

    public async Task<Result<BannerDto>> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken = default)
    {
        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ImageUrl = dto.ImageUrl,
            TargetUrl = dto.TargetUrl,
            Position = dto.Position,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<BannerDto>(banner));
    }

    public async Task<Result<BannerDto>> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken = default)
    {
        var banner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (banner == null)
            return Result.Failure<BannerDto>(Error.NotFound("Banner", id));

        banner.Title = dto.Title;
        banner.Subtitle = dto.Subtitle;
        banner.ImageUrl = dto.ImageUrl;
        banner.TargetUrl = dto.TargetUrl;
        banner.Position = dto.Position;
        banner.DisplayOrder = dto.DisplayOrder;
        banner.IsActive = dto.IsActive;
        banner.StartDate = dto.StartDate;
        banner.EndDate = dto.EndDate;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(_mapper.Map<BannerDto>(banner));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var banner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (banner == null)
            return Result.Failure(Error.NotFound("Banner", id));

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

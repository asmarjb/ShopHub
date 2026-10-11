using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Brands.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Brands.Services;

public partial class BrandService : IBrandService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public BrandService(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<BrandDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Brands.AsNoTracking();
        if (!includeInactive)
            query = query.Where(b => b.IsActive);

        var brands = await query.OrderBy(b => b.Name).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<BrandDto>>(_mapper.Map<List<BrandDto>>(brands));
    }

    public async Task<Result<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (brand == null)
            return Result.Failure<BrandDto>(Error.NotFound("Brand", id));

        return Result.Success(_mapper.Map<BrandDto>(brand));
    }

    public async Task<Result<BrandDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);
        if (brand == null)
            return Result.Failure<BrandDto>($"Brand with slug '{slug}' was not found.");

        return Result.Success(_mapper.Map<BrandDto>(brand));
    }

    public async Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(dto.Name);
        if (await _context.Brands.AnyAsync(b => b.Slug == slug, cancellationToken))
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 9, 150)];

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            LogoUrl = dto.LogoUrl,
            IsActive = dto.IsActive
        };

        _context.Brands.Add(brand);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Brand",
            EntityId = brand.Id.ToString(),
            Action = "CreateBrand",
            NewValues = JsonSerializer.Serialize(new { brand.Name, brand.Slug, brand.IsActive }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(_mapper.Map<BrandDto>(brand));
    }

    public async Task<Result<BrandDto>> UpdateAsync(Guid id, UpdateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (brand == null)
            return Result.Failure<BrandDto>(Error.NotFound("Brand", id));

        var oldValues = JsonSerializer.Serialize(new { brand.Name, brand.Description, brand.LogoUrl, brand.IsActive });

        brand.Name = dto.Name;
        brand.Description = dto.Description;
        brand.LogoUrl = dto.LogoUrl;
        brand.IsActive = dto.IsActive;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Brand",
            EntityId = brand.Id.ToString(),
            Action = "UpdateBrand",
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(new { brand.Name, brand.Description, brand.LogoUrl, brand.IsActive }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(_mapper.Map<BrandDto>(brand));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (brand == null)
            return Result.Failure(Error.NotFound("Brand", id));

        var linkedProductCount = await _context.Products.CountAsync(p => p.BrandId == id, cancellationToken);
        if (linkedProductCount > 0)
            return Result.Failure($"Cannot delete brand '{brand.Name}'. It has {linkedProductCount} linked product(s). Please reassign or remove the brand from those products first.");

        _context.Brands.Remove(brand);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Brand",
            EntityId = brand.Id.ToString(),
            Action = "DeleteBrand",
            OldValues = JsonSerializer.Serialize(new { brand.Name, brand.Slug }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static string GenerateSlug(string text)
    {
        var str = text.ToLowerInvariant().Trim();
        str = SlugRegex().Replace(str, "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(str) ? "brand" : str;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();
}

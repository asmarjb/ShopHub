using System.Text.Json;
using System.Text.RegularExpressions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Tags.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Tags.Services;

public partial class TagService : ITagService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TagService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<TagDto>>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _context.Tags
            .Include(t => t.ProductTags)
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug, t.ProductTags.Count))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TagDto>>(tags);
    }

    public async Task<Result<TagDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _context.Tags
            .Include(t => t.ProductTags)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag == null)
            return Result.Failure<TagDto>(Error.NotFound("Tag", id));

        return Result.Success(new TagDto(tag.Id, tag.Name, tag.Slug, tag.ProductTags.Count));
    }

    public async Task<Result<TagDto>> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default)
    {
        var trimmedName = dto.Name.Trim();
        var slug = GenerateSlug(trimmedName);

        if (await _context.Tags.AnyAsync(t => t.Name.ToLower() == trimmedName.ToLower() || t.Slug == slug, cancellationToken))
            return Result.Failure<TagDto>($"Tag '{trimmedName}' already exists.");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Slug = slug
        };

        _context.Tags.Add(tag);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Tag",
            EntityId = tag.Id.ToString(),
            Action = "CreateTag",
            NewValues = JsonSerializer.Serialize(new { tag.Name, tag.Slug }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(new TagDto(tag.Id, tag.Name, tag.Slug, 0));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _context.Tags
            .Include(t => t.ProductTags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag == null)
            return Result.Failure(Error.NotFound("Tag", id));

        _context.Tags.Remove(tag);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Tag",
            EntityId = tag.Id.ToString(),
            Action = "DeleteTag",
            OldValues = JsonSerializer.Serialize(new { tag.Name, tag.Slug }),
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
        return string.IsNullOrEmpty(str) ? "tag" : str;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();
}

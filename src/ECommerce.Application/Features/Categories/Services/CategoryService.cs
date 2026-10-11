using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Categories.Services;

public partial class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CategoryService(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CategoryDto>>(_mapper.Map<List<CategoryDto>>(categories));
    }

    public async Task<Result<IReadOnlyList<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var rootCategories = allCategories
            .Where(c => c.ParentCategoryId == null)
            .Select(c => BuildTree(c, allCategories))
            .ToList();

        return Result.Success<IReadOnlyList<CategoryTreeDto>>(rootCategories);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category", id));

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        if (category == null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category", slug));

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(dto.Name);
        var slugExists = await _context.Categories.AnyAsync(c => c.Slug == slug, cancellationToken);
        if (slugExists)
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 9, 150)];

        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Result.Failure<CategoryDto>("Specified parent category does not exist.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(category.Id, cancellationToken);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category", id));

        if (dto.ParentCategoryId == id)
            return Result.Failure<CategoryDto>("A category cannot be its own parent.");

        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Result.Failure<CategoryDto>("Specified parent category does not exist.");
        }

        if (category.Name != dto.Name)
        {
            var newSlug = GenerateSlug(dto.Name);
            if (await _context.Categories.AnyAsync(c => c.Slug == newSlug && c.Id != id, cancellationToken))
                newSlug = $"{newSlug}-{Guid.NewGuid():N}"[..Math.Min(newSlug.Length + 9, 150)];
            category.Slug = newSlug;
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ImageUrl = dto.ImageUrl;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(category.Id, cancellationToken);
    }

    public async Task<Result<CategoryDeleteImpactDto>> GetDeleteImpactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null)
            return Result.Failure<CategoryDeleteImpactDto>(Error.NotFound("Category", id));

        var activeProductsCount = await _context.Products.CountAsync(p => p.CategoryId == id, cancellationToken);
        var childCategoriesCount = await _context.Categories.CountAsync(c => c.ParentCategoryId == id, cancellationToken);
        var totalProductsCount = await _context.Products.IgnoreQueryFilters().CountAsync(p => p.CategoryId == id, cancellationToken);

        return Result.Success(new CategoryDeleteImpactDto(category.Id, category.Name, activeProductsCount, childCategoriesCount, totalProductsCount));
    }

    public async Task<Result> SoftDeleteAsync(Guid id, Guid? reassignToCategoryId = null, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            return Result.Failure(Error.NotFound("Category", id));

        var activeProductsCount = await _context.Products.CountAsync(p => p.CategoryId == id, cancellationToken);
        var childCategoriesCount = await _context.Categories.CountAsync(c => c.ParentCategoryId == id, cancellationToken);

        if (!reassignToCategoryId.HasValue)
        {
            if (activeProductsCount > 0 || childCategoriesCount > 0)
            {
                return Result.Failure($"Cannot delete category '{category.Name}'. It has {activeProductsCount} active product(s) and {childCategoriesCount} child category(ies). Please select a target category to reassign them to ('reassignToCategoryId') or delete them first.");
            }
        }
        else
        {
            if (reassignToCategoryId.Value == id)
            {
                return Result.Failure("Cannot reassign products to the category being deleted.");
            }

            var targetCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == reassignToCategoryId.Value, cancellationToken);
            if (targetCategory == null)
            {
                return Result.Failure(Error.NotFound("Target Category for reassignment", reassignToCategoryId.Value));
            }

            // Move products
            var productsToMove = await _context.Products.Where(p => p.CategoryId == id).ToListAsync(cancellationToken);
            foreach (var product in productsToMove)
            {
                product.CategoryId = targetCategory.Id;
            }

            // Move child categories
            var subCategoriesToMove = await _context.Categories.Where(c => c.ParentCategoryId == id).ToListAsync(cancellationToken);
            foreach (var subCat in subCategoriesToMove)
            {
                subCat.ParentCategoryId = targetCategory.Id;
            }

            // Record audit log for reassignment
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Category",
                EntityId = category.Id.ToString(),
                Action = "ReassignCategoryProducts",
                OldValues = JsonSerializer.Serialize(new { category.Name, ProductsCount = productsToMove.Count, SubCategoriesCount = subCategoriesToMove.Count }),
                NewValues = JsonSerializer.Serialize(new { TargetCategoryId = targetCategory.Id, TargetCategoryName = targetCategory.Name }),
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName ?? "system",
                IpAddress = _currentUserService.IpAddress,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Mark category as soft deleted
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = _currentUserService.UserName ?? "system";

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Category",
            EntityId = category.Id.ToString(),
            Action = "SoftDelete",
            OldValues = JsonSerializer.Serialize(new { category.Name, category.Slug }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted, cancellationToken);

        if (category == null)
            return Result.Failure(Error.NotFound("Deleted Category", id));

        if (category.ParentCategoryId.HasValue)
        {
            var parentCategory = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId.Value, cancellationToken);

            if (parentCategory == null || parentCategory.IsDeleted)
            {
                return Result.Failure("Cannot restore category because its parent category is currently in the recycle bin. Please restore the parent category first.");
            }
        }

        category.IsDeleted = false;
        category.DeletedAt = null;
        category.DeletedBy = null;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Category",
            EntityId = category.Id.ToString(),
            Action = "Restore",
            NewValues = JsonSerializer.Serialize(new { category.Name }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> HardDeleteAsync(Guid id, bool confirm, CancellationToken cancellationToken = default)
    {
        if (!confirm)
            return Result.Failure("Hard delete requires explicit confirmation (confirm=true). Please pass confirm=true to proceed.");

        var category = await _context.Categories
            .IgnoreQueryFilters()
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            return Result.Failure(Error.NotFound("Category", id));

        var totalProductReferences = await _context.Products
            .IgnoreQueryFilters()
            .CountAsync(p => p.CategoryId == id, cancellationToken);

        if (totalProductReferences > 0)
        {
            return Result.Failure($"Cannot permanently delete category '{category.Name}'. There are {totalProductReferences} product(s) (including in recycle bin) referencing this category. Clean them up or reassign first.");
        }

        var totalChildReferences = await _context.Categories
            .IgnoreQueryFilters()
            .CountAsync(c => c.ParentCategoryId == id, cancellationToken);

        if (totalChildReferences > 0)
        {
            return Result.Failure($"Cannot permanently delete category '{category.Name}'. There are {totalChildReferences} child category(ies) (including in recycle bin) referencing this category.");
        }

        _context.Categories.Remove(category);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Category",
            EntityId = category.Id.ToString(),
            Action = "HardDelete",
            OldValues = JsonSerializer.Serialize(new { category.Name, category.Slug }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetDeletedCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var deletedCategories = await _context.Categories
            .IgnoreQueryFilters()
            .Include(c => c.ParentCategory)
            .Where(c => c.IsDeleted)
            .OrderByDescending(c => c.DeletedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CategoryDto>>(_mapper.Map<List<CategoryDto>>(deletedCategories));
    }

    private static CategoryTreeDto BuildTree(Category current, List<Category> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentCategoryId == current.Id)
            .Select(c => BuildTree(c, allCategories))
            .ToList();

        return new CategoryTreeDto(
            current.Id,
            current.Name,
            current.Slug,
            current.ImageUrl,
            current.DisplayOrder,
            children);
    }

    private static string GenerateSlug(string text)
    {
        var str = text.ToLowerInvariant().Trim();
        str = SlugRegex().Replace(str, "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(str) ? "item" : str;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();
}

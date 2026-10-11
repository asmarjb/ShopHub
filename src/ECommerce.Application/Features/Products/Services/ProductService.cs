using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Products.Services;

public partial class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ProductService(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<ProductListDto>>> GetProductsAsync(ProductFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags)
            .AsNoTracking()
            .Where(p => p.IsActive);

        // Search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search)
                                  || p.SKU.ToLower().Contains(search)
                                  || (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(search)));
        }

        // Category filter
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value || p.Category.ParentCategoryId == filter.CategoryId.Value);
        }

        // Brand filter
        if (filter.BrandId.HasValue)
        {
            query = query.Where(p => p.BrandId == filter.BrandId.Value);
        }

        // Tag filter
        if (filter.TagId.HasValue)
        {
            query = query.Where(p => p.ProductTags.Any(pt => pt.TagId == filter.TagId.Value));
        }

        // Price filter
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);
        }

        // In Stock filter
        if (filter.InStockOnly == true)
        {
            query = query.Where(p => p.Variants.Any(v => v.StockQuantity > 0));
        }

        // Featured filter
        if (filter.FeaturedOnly == true)
        {
            query = query.Where(p => p.IsFeatured);
        }

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "price" => filter.IsDescending ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "name" => filter.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.SKU,
                p.ShortDescription,
                p.BasePrice,
                p.CompareAtPrice,
                p.Category.Name,
                p.CategoryId,
                p.Brand != null ? p.Brand.Name : null,
                p.BrandId,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.AverageRating,
                p.ReviewCount,
                p.IsFeatured,
                p.IsActive,
                p.IsDeleted,
                p.Variants.Sum(v => v.StockQuantity)))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<ProductListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize));
    }

    public async Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            return Result.Failure<ProductDetailDto>(Error.NotFound("Product", id));

        return Result.Success(MapToDetailDto(product));
    }

    public async Task<Result<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

        if (product == null)
            return Result.Failure<ProductDetailDto>(Error.NotFound("Product", slug));

        return Result.Success(MapToDetailDto(product));
    }

    public async Task<Result<ProductDetailDto>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (await _context.Products.AnyAsync(p => p.SKU == dto.SKU, cancellationToken))
            return Result.Failure<ProductDetailDto>($"Product with SKU '{dto.SKU}' already exists.");

        var slug = GenerateSlug(dto.Name);
        if (await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken))
            slug = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 9, 200)];

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = slug,
            SKU = dto.SKU,
            ShortDescription = dto.ShortDescription,
            FullDescription = dto.FullDescription,
            BasePrice = dto.BasePrice,
            CompareAtPrice = dto.CompareAtPrice,
            CostPrice = dto.CostPrice,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            IsFeatured = dto.IsFeatured,
            IsActive = dto.IsActive
        };

        if (dto.Variants != null && dto.Variants.Any())
        {
            int index = 1;
            foreach (var v in dto.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    SKU = $"{product.SKU}-{index++}",
                    Name = v.Name,
                    Color = v.Color,
                    Size = v.Size,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    LowStockThreshold = v.LowStockThreshold,
                    ImageUrl = v.ImageUrl,
                    Barcode = v.Barcode
                };
                product.Variants.Add(variant);
            }
        }
        else
        {
            // Default variant
            product.Variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                SKU = $"{product.SKU}-DEFAULT",
                Name = "Standard",
                Price = product.BasePrice,
                StockQuantity = 0,
                LowStockThreshold = 5
            });
        }

        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var img in dto.Images)
            {
                product.Images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ImageUrl = img.ImageUrl,
                    AltText = img.AltText,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder
                });
            }
        }

        if (dto.TagIds != null && dto.TagIds.Any())
        {
            foreach (var tagId in dto.TagIds)
            {
                product.ProductTags.Add(new ProductTag
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    TagId = tagId
                });
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<Result<ProductDetailDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.ProductTags)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            return Result.Failure<ProductDetailDto>(Error.NotFound("Product", id));

        if (product.SKU != dto.SKU && await _context.Products.AnyAsync(p => p.SKU == dto.SKU && p.Id != id, cancellationToken))
            return Result.Failure<ProductDetailDto>($"Product with SKU '{dto.SKU}' already exists.");

        if (product.Name != dto.Name)
        {
            var newSlug = GenerateSlug(dto.Name);
            if (await _context.Products.AnyAsync(p => p.Slug == newSlug && p.Id != id, cancellationToken))
                newSlug = $"{newSlug}-{Guid.NewGuid():N}"[..Math.Min(newSlug.Length + 9, 200)];
            product.Slug = newSlug;
        }

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.ShortDescription = dto.ShortDescription;
        product.FullDescription = dto.FullDescription;
        product.BasePrice = dto.BasePrice;
        product.CompareAtPrice = dto.CompareAtPrice;
        product.CostPrice = dto.CostPrice;
        product.CategoryId = dto.CategoryId;
        product.BrandId = dto.BrandId;
        product.IsFeatured = dto.IsFeatured;
        product.IsActive = dto.IsActive;

        if (dto.TagIds != null)
        {
            product.ProductTags.Clear();
            foreach (var tagId in dto.TagIds)
            {
                product.ProductTags.Add(new ProductTag { Id = Guid.NewGuid(), ProductId = product.Id, TagId = tagId });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<Result> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product == null)
            return Result.Failure(Error.NotFound("Product", id));

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = _currentUserService.UserName ?? "system";

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Product",
            EntityId = product.Id.ToString(),
            Action = "SoftDelete",
            OldValues = JsonSerializer.Serialize(new { product.Name, product.SKU, product.BasePrice }),
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
        var product = await _context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted, cancellationToken);

        if (product == null)
            return Result.Failure(Error.NotFound("Deleted Product", id));

        // Check if category is deleted
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == product.CategoryId, cancellationToken);

        if (category == null || category.IsDeleted)
            return Result.Failure($"Cannot restore product '{product.Name}' because its assigned category is currently in the recycle bin. Please restore the category first.");

        product.IsDeleted = false;
        product.DeletedAt = null;
        product.DeletedBy = null;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Product",
            EntityId = product.Id.ToString(),
            Action = "Restore",
            NewValues = JsonSerializer.Serialize(new { product.Name, product.SKU }),
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
            return Result.Failure("Hard delete requires explicit confirmation (confirm=true). Order items will preserve product snapshots, but the catalog item will be erased permanently.");

        var product = await _context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.ProductTags)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            return Result.Failure(Error.NotFound("Product", id));

        _context.Products.Remove(product);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Product",
            EntityId = product.Id.ToString(),
            Action = "HardDelete",
            OldValues = JsonSerializer.Serialize(new { product.Name, product.SKU, product.BasePrice }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedResult<ProductListDto>>> GetDeletedProductsAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.IsDeleted)
            .OrderByDescending(p => p.DeletedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.SKU,
                p.ShortDescription,
                p.BasePrice,
                p.CompareAtPrice,
                p.Category.Name,
                p.CategoryId,
                p.Brand != null ? p.Brand.Name : null,
                p.BrandId,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.AverageRating,
                p.ReviewCount,
                p.IsFeatured,
                p.IsActive,
                p.IsDeleted,
                p.Variants.Sum(v => v.StockQuantity)))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<ProductListDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<IReadOnlyList<ProductListDto>>> GetRelatedProductsAsync(Guid productId, int count = 4, CancellationToken cancellationToken = default)
    {
        var current = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (current == null)
            return Result.Failure<IReadOnlyList<ProductListDto>>(Error.NotFound("Product", productId));

        var related = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsNoTracking()
            .Where(p => p.Id != productId && p.IsActive && p.CategoryId == current.CategoryId)
            .Take(count)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.SKU,
                p.ShortDescription,
                p.BasePrice,
                p.CompareAtPrice,
                p.Category.Name,
                p.CategoryId,
                p.Brand != null ? p.Brand.Name : null,
                p.BrandId,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.AverageRating,
                p.ReviewCount,
                p.IsFeatured,
                p.IsActive,
                p.IsDeleted,
                p.Variants.Sum(v => v.StockQuantity)))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductListDto>>(related);
    }

    public async Task<Result<IReadOnlyList<ProductVariantDto>>> GetLowStockVariantsAsync(CancellationToken cancellationToken = default)
    {
        var lowStock = await _context.ProductVariants
            .Include(v => v.Product)
            .AsNoTracking()
            .Where(v => v.StockQuantity <= v.LowStockThreshold)
            .Select(v => new ProductVariantDto(
                v.Id,
                v.ProductId,
                v.SKU,
                $"{v.Product.Name} - {v.Name}",
                v.Color,
                v.Size,
                v.Price,
                v.StockQuantity,
                v.LowStockThreshold,
                v.StockQuantity > 0,
                true,
                v.ImageUrl,
                v.Barcode))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductVariantDto>>(lowStock);
    }

    public async Task<Result<IReadOnlyList<SearchAutocompleteDto>>> AutocompleteAsync(string query, int count = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Result.Success<IReadOnlyList<SearchAutocompleteDto>>(new List<SearchAutocompleteDto>());

        var search = query.Trim().ToLower();
        var matches = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsNoTracking()
            .Where(p => p.IsActive && (p.Name.ToLower().Contains(search) || p.SKU.ToLower().Contains(search)))
            .Take(count)
            .Select(p => new SearchAutocompleteDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.BasePrice,
                p.Category.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SearchAutocompleteDto>>(matches);
    }

    public async Task<Result<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            return Result.Failure<ProductVariantDto>(Error.NotFound("Product", productId));

        var nextIndex = product.Variants.Count + 1;
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SKU = $"{product.SKU}-{nextIndex}",
            Name = dto.Name,
            Color = dto.Color,
            Size = dto.Size,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            ImageUrl = dto.ImageUrl,
            Barcode = dto.Barcode
        };

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProductVariantDto(
            variant.Id,
            variant.ProductId,
            variant.SKU,
            variant.Name,
            variant.Color,
            variant.Size,
            variant.Price,
            variant.StockQuantity,
            variant.LowStockThreshold,
            variant.IsInStock,
            variant.IsLowStock,
            variant.ImageUrl,
            variant.Barcode));
    }

    public async Task<Result> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
        if (variant == null)
            return Result.Failure(Error.NotFound("ProductVariant", variantId));

        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProductListDto>>> GetRecentlyViewedAsync(List<Guid> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds == null || productIds.Count == 0)
            return Result.Success<IReadOnlyList<ProductListDto>>(new List<ProductListDto>());

        var distinctIds = productIds.Distinct().Take(20).ToList();

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsNoTracking()
            .Where(p => distinctIds.Contains(p.Id) && p.IsActive)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.SKU,
                p.ShortDescription,
                p.BasePrice,
                p.CompareAtPrice,
                p.Category.Name,
                p.CategoryId,
                p.Brand != null ? p.Brand.Name : null,
                p.BrandId,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.AverageRating,
                p.ReviewCount,
                p.IsFeatured,
                p.IsActive,
                p.IsDeleted,
                p.Variants.Sum(v => v.StockQuantity)))
            .ToListAsync(cancellationToken);

        // Preserve input order
        var ordered = distinctIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Cast<ProductListDto>()
            .ToList();

        return Result.Success<IReadOnlyList<ProductListDto>>(ordered);
    }

    public async Task<Result<ProductImageDto>> AddImageAsync(Guid productId, string imageUrl, string? altText, bool isPrimary, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
            return Result.Failure<ProductImageDto>(Error.NotFound("Product", productId));

        // If marked as primary, reset previous primary images
        if (isPrimary || product.Images.Count == 0)
        {
            isPrimary = true;
            foreach (var img in product.Images)
            {
                img.IsPrimary = false;
            }
        }

        var nextOrder = product.Images.Count + 1;
        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImageUrl = imageUrl,
            AltText = altText ?? product.Name,
            IsPrimary = isPrimary,
            DisplayOrder = nextOrder
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProductImageDto(image.Id, image.ImageUrl, image.AltText, image.IsPrimary, image.DisplayOrder));
    }

    public async Task<Result> SetPrimaryImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var images = await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

        if (!images.Any())
            return Result.Failure(Error.NotFound("ProductImages for product", productId));

        var target = images.FirstOrDefault(i => i.Id == imageId);
        if (target == null)
            return Result.Failure(Error.NotFound("ProductImage", imageId));

        foreach (var img in images)
        {
            img.IsPrimary = (img.Id == imageId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, cancellationToken);

        if (image == null)
            return Result.Failure(Error.NotFound("ProductImage", imageId));

        var wasPrimary = image.IsPrimary;
        _context.ProductImages.Remove(image);

        if (wasPrimary)
        {
            var nextImage = await _context.ProductImages
                .Where(i => i.ProductId == productId && i.Id != imageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextImage != null)
            {
                nextImage.IsPrimary = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static ProductDetailDto MapToDetailDto(Product p)
    {
        return new ProductDetailDto(
            p.Id,
            p.Name,
            p.Slug,
            p.SKU,
            p.ShortDescription,
            p.FullDescription,
            p.BasePrice,
            p.CompareAtPrice,
            p.CostPrice,
            p.CategoryId,
            p.Category.Name,
            p.BrandId,
            p.Brand?.Name,
            p.IsActive,
            p.IsFeatured,
            p.AverageRating,
            p.ReviewCount,
            p.IsDeleted,
            p.DeletedAt,
            p.DeletedBy,
            p.Variants.Select(v => new ProductVariantDto(
                v.Id,
                v.ProductId,
                v.SKU,
                v.Name,
                v.Color,
                v.Size,
                v.Price,
                v.StockQuantity,
                v.LowStockThreshold,
                v.IsInStock,
                v.IsLowStock,
                v.ImageUrl,
                v.Barcode)).ToList(),
            p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto(
                i.Id,
                i.ImageUrl,
                i.AltText,
                i.IsPrimary,
                i.DisplayOrder)).ToList(),
            p.ProductTags.Select(pt => pt.Tag.Name).ToList());
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

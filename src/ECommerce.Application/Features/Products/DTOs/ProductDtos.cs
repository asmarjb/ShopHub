using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Features.Products.DTOs;

public record ProductVariantDto(
    Guid Id,
    Guid ProductId,
    string SKU,
    string Name,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    bool IsInStock,
    bool IsLowStock,
    string? ImageUrl,
    string? Barcode);

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    string? AltText,
    bool IsPrimary,
    int DisplayOrder);

public record ProductListDto(
    Guid Id,
    string Name,
    string Slug,
    string SKU,
    string? ShortDescription,
    decimal BasePrice,
    decimal? CompareAtPrice,
    string? CategoryName,
    Guid CategoryId,
    string? BrandName,
    Guid? BrandId,
    string? PrimaryImageUrl,
    double AverageRating,
    int ReviewCount,
    bool IsFeatured,
    bool IsActive,
    bool IsDeleted,
    int TotalStock);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string SKU,
    string? ShortDescription,
    string? FullDescription,
    decimal BasePrice,
    decimal? CompareAtPrice,
    decimal? CostPrice,
    Guid CategoryId,
    string CategoryName,
    Guid? BrandId,
    string? BrandName,
    bool IsActive,
    bool IsFeatured,
    double AverageRating,
    int ReviewCount,
    bool IsDeleted,
    DateTime? DeletedAt,
    string? DeletedBy,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images,
    List<string> Tags);

public record CreateProductVariantDto(
    string Name,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold = 5,
    string? ImageUrl = null,
    string? Barcode = null);

public record CreateProductImageDto(
    string ImageUrl,
    string? AltText,
    bool IsPrimary,
    int DisplayOrder);

public record CreateProductDto(
    string Name,
    string SKU,
    string? ShortDescription,
    string? FullDescription,
    decimal BasePrice,
    decimal? CompareAtPrice,
    decimal? CostPrice,
    Guid CategoryId,
    Guid? BrandId,
    bool IsFeatured,
    bool IsActive = true,
    List<CreateProductVariantDto>? Variants = null,
    List<CreateProductImageDto>? Images = null,
    List<Guid>? TagIds = null);

public record UpdateProductDto(
    string Name,
    string SKU,
    string? ShortDescription,
    string? FullDescription,
    decimal BasePrice,
    decimal? CompareAtPrice,
    decimal? CostPrice,
    Guid CategoryId,
    Guid? BrandId,
    bool IsFeatured,
    bool IsActive,
    List<Guid>? TagIds = null);

public class ProductFilterParams : PaginationParams
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? TagId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? FeaturedOnly { get; set; }
}

public record SearchAutocompleteDto(
    Guid Id,
    string Name,
    string Slug,
    string? PrimaryImageUrl,
    decimal BasePrice,
    string CategoryName);

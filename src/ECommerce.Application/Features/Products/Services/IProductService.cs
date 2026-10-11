using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Products.DTOs;

namespace ECommerce.Application.Features.Products.Services;

public interface IProductService
{
    Task<Result<PagedResult<ProductListDto>>> GetProductsAsync(ProductFilterParams filter, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailDto>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> HardDeleteAsync(Guid id, bool confirm, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductListDto>>> GetDeletedProductsAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProductListDto>>> GetRelatedProductsAsync(Guid productId, int count = 4, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProductVariantDto>>> GetLowStockVariantsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SearchAutocompleteDto>>> AutocompleteAsync(string query, int count = 5, CancellationToken cancellationToken = default);
    Task<Result<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProductListDto>>> GetRecentlyViewedAsync(List<Guid> productIds, CancellationToken cancellationToken = default);
    Task<Result<ProductImageDto>> AddImageAsync(Guid productId, string imageUrl, string? altText, bool isPrimary, CancellationToken cancellationToken = default);
    Task<Result> SetPrimaryImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
    Task<Result> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
}

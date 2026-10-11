using AutoMapper;
using ECommerce.Application.Features.Banners.DTOs;
using ECommerce.Application.Features.Brands.DTOs;
using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Features.Shipping.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category Mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryName, opt => opt.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));

        // Brand Mappings
        CreateMap<Brand, BrandDto>();

        // Shipping Mappings
        CreateMap<ShippingMethod, ShippingMethodDto>();

        // Banner Mappings
        CreateMap<Banner, BannerDto>();

        // Product Variant & Image Mappings
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(d => d.IsInStock, opt => opt.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.IsLowStock, opt => opt.MapFrom(s => s.StockQuantity > 0 && s.StockQuantity <= s.LowStockThreshold));

        CreateMap<ProductImage, ProductImageDto>();
    }
}

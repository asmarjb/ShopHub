using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Features.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterParams filter, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _productService.GetBySlugAsync(slug, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string q, [FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var result = await _productService.AutocompleteAsync(q, count, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/related")]
    public async Task<IActionResult> GetRelatedProducts(Guid id, [FromQuery] int count = 4, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetRelatedProductsAsync(id, count, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("recently-viewed")]
    public async Task<IActionResult> GetRecentlyViewed([FromBody] List<Guid> productIds, CancellationToken cancellationToken)
    {
        var result = await _productService.GetRecentlyViewedAsync(productIds, cancellationToken);
        return HandleResult(result);
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Controllers;

namespace Shop.Products.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(
    IProductQueryService productQueryService,
    IProductCommandService productCommandService
    ) : ApiBaseController
{
    /// <summary>
    /// POST api/products — creates a new product for the authenticated user
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddNewProduct(ProductInfoDto dto, IValidator<ProductInfoDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var userId = GetCurrentUserId();
        
        var id = await productCommandService.CreateProduct(userId, dto);
        return CreatedAtAction(nameof(GetProduct), new { productId = id }, null);
    }

    /// <summary>
    /// PUT api/products/{productId} — updates name, description and price of the caller's product
    /// </summary>
    [Authorize]
    [HttpPut("{productId}")]
    public async Task<IActionResult> EditProduct(Guid productId, ProductInfoDto dto, IValidator<ProductInfoDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var userId = GetCurrentUserId();

        await productCommandService.EditProduct(productId, userId, dto);
        
        return Ok();
    }
    
    /// <summary>
    /// POST api/products/{productId}/deactivate — marks the caller's product as unavailable
    /// </summary>
    [Authorize]
    [HttpPost("{productId}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid productId)
    {
        var userId = GetCurrentUserId();
        
        await productCommandService.DeactivateProduct(productId, userId);

        return Ok();
    }
    
    /// <summary>
    /// POST api/products/{productId}/activate — marks the caller's product as available
    /// </summary>
    [Authorize]
    [HttpPost("{productId}/activate")]
    public async Task<IActionResult> ActivateProduct(Guid productId)
    {
        var userId = GetCurrentUserId();

        await productCommandService.ActivateProduct(productId, userId);

        return Ok();
    }

    /// <summary>
    /// GET api/products/{productId} — returns a single product by id
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        var product = await productQueryService.GetProductById(productId);

        return Ok(product);
    }
    
    /// <summary>
    /// GET api/products/all — returns all active products, supports filtering by name, price range and owner
    /// </summary>
    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts([FromQuery] ProductFilterDto filter)
    {
        var products = await productQueryService.GetAllProducts(filter, false);

        return Ok(products);
    }
    
    /// <summary>
    /// GET api/products/my-products — returns all products belonging to the authenticated user, including deactivated ones
    /// </summary>
    [Authorize]
    [HttpGet("my-products")]
    public async Task<IActionResult> GetAllProductsForUser([FromQuery] bool showDeleted = false)
    {
        var userId = GetCurrentUserId();
        var filter = new ProductFilterDto
        {
            UserId = userId
        };

        var products = await productQueryService.GetAllProducts(filter, showDeleted);

        return Ok(products);
    }
}
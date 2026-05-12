using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Products.DTOs;
using Shop.Products.Services.Interfaces;
using Shop.Shared.Exceptions;
using Shop.Shared.Middleware;

namespace Shop.Products.Controllers;

[ApiController]
[Route("api/products")]
public partial class ProductsController(IProductService productService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddNewProduct(ProductInfoDto dto, IValidator<ProductInfoDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var userId = GetCurrentUserId();
        
        var id = await productService.CreateProduct(userId, dto);
        return CreatedAtAction(nameof(GetProduct), new { productId = id }, null);
    }

    [Authorize]
    [HttpPut("{productId}")]
    public async Task<IActionResult> EditProduct(Guid productId, ProductInfoDto dto, IValidator<ProductInfoDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var userId = GetCurrentUserId();

        await productService.EditProduct(productId, userId, dto);
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("{productId}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid productId)
    {
        var userId = GetCurrentUserId();
        
        await productService.DeactivateProduct(productId, userId);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("{productId}/activate")]
    public async Task<IActionResult> ActivateProduct(Guid productId)
    {
        var userId = GetCurrentUserId();

        await productService.ActivateProduct(productId, userId);

        return Ok();
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        var product = await productService.GetProductById(productId);

        return Ok(product);
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts([FromQuery] ProductFilterDto filter)
    {
        var products = await productService.GetAllProducts(filter);

        return Ok(products);
    }
    
    [Authorize]
    [HttpGet("my-products")]
    public async Task<IActionResult> GetAllProductsForUser([FromQuery] bool showDeleted = false)
    {
        var userId = GetCurrentUserId();
        var filter = new ProductFilterDto
        {
            UserId = userId
        };

        var products = await productService.GetAllProducts(filter, showDeleted);

        return Ok(products);
    }

    [InternalApiKey]
    [HttpPost("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await productService.SoftDeleteUserProducts(id);
        return Ok();
    }
    
    [InternalApiKey]
    [HttpPost("users/{id}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid id)
    {
        await productService.RestoreUserProducts(id);
        return Ok();
    }
    
    [InternalApiKey]
    [HttpDelete("users/{id}/delete")]
    public async Task<IActionResult> DeleteAllUserProducts(Guid id)
    {
        await productService.DeleteAllProductsForUser(id);
        return NoContent();
    }
}
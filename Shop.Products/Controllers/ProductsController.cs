using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Products.DTOs;
using Shop.Products.Services.Interfaces;
using Shop.Shared.Exceptions;

namespace Shop.Products.Controllers;

[ApiController]
[Route("api/products")]
public partial class ProductsController(IProductService productService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddNewProduct(ProductCreateDto dto, IValidator<ProductCreateDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var userId = GetCurrentUserId();
        
        await productService.CreateProduct(userId, dto);
        
        return Ok();
    }

    [Authorize]
    [HttpDelete("{productId}/delete")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var userId = GetCurrentUserId();

        await productService.DeleteProduct(productId, userId);
        
        return NoContent();
    }

    [Authorize]
    [HttpPut("{productId}")]
    public async Task<IActionResult> EditProduct(Guid productId, ProductChangeInfoDto dto, IValidator<ProductChangeInfoDto> validator)
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
    public async Task<IActionResult> GetAllProducts(ProductFilterDto filter)
    {
        var products = await productService.GetAllProducts(filter);

        return Ok(products);
    }
    
    [Authorize]
    [HttpGet("my-products")]
    public async Task<IActionResult> GetAllProductsForUser()
    {
        var userId = GetCurrentUserId();
        var filter = new ProductFilterDto
        {
            UserId = userId
        };

        var products = await productService.GetAllProducts(filter, true);

        return Ok(products);
    }
}
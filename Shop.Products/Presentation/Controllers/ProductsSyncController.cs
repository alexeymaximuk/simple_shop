using Microsoft.AspNetCore.Mvc;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Controllers;
using Shop.Shared.Filters;

namespace Shop.Products.Presentation.Controllers;

[ApiController]
[Route("api/products/users")]
public class ProductsSyncController(IProductExternalServices productExternalServices) : ApiBaseController
{
    /// <summary>
    /// POST api/products/users/{id}/deactivate — hides all products of a user (called internally by Users service on user deactivation)
    /// </summary>
    [InternalApiKey]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await productExternalServices.SoftDeleteUserProducts(id);
        return Ok();
    }

    /// <summary>
    /// POST api/products/users/{id}/reactivate — restores all products of a user (called internally by Users service on user reactivation)
    /// </summary>
    [InternalApiKey]
    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid id)
    {
        await productExternalServices.RestoreUserProducts(id);
        return Ok();
    }

    /// <summary>
    /// DELETE api/products/users/{id}/delete — permanently deletes all products of a user (called internally by Users service on user deletion)
    /// </summary>
    [InternalApiKey]
    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> DeleteAllUserProducts(Guid id)
    {
        await productExternalServices.DeleteAllProductsForUser(id);
        return NoContent();
    }
}

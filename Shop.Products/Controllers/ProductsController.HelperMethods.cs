using System.Security.Claims;
using Shop.Shared.Exceptions;

namespace Shop.Products.Controllers;

public partial class ProductsController
{
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new AuthorisationException("User not authorized");

        return Guid.Parse(claim);
    }
}
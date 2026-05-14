using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shop.Shared.Exceptions;

namespace Shop.Shared.Controllers;

/// <summary>
/// Base controller for API microservices. Provides common helper methods for API controllers.
/// </summary>
public abstract class ApiBaseController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new AuthorisationException("User not authorized");

        return Guid.TryParse(claim, out var id)
            ? id
            : throw new AuthorisationException("User not authorized");
    }
}


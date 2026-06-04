using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Shop.Shared.Constants;
using Shop.Shared.Exceptions;
using Shop.Shared.Interfaces;

namespace Shop.Shared.Middleware;

public class SessionValidationMiddleware(RequestDelegate next, IRedisSessionService redisService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endPoint = context.GetEndpoint();

        if (!context.User.Identity?.IsAuthenticated == true ||
            endPoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next(context);
            return;
        }

        var jti = context.User.FindFirstValue(AuthConstants.ClaimNames.Jti);

        if (jti == null || !Guid.TryParse(jti, out var jtiGuid))
            throw new SessionExpiredException("Session expired");

        var session = await redisService.GetSessionAsync(jtiGuid);

        if (session == null)
            throw new SessionExpiredException("Session expired");
        
        if (session != context.User.FindFirstValue(AuthConstants.ClaimNames.Sub))
            throw new SessionExpiredException("Session expired");
        
        await redisService.RefreshSessionAsync(jtiGuid);
        await next(context);
    }
}
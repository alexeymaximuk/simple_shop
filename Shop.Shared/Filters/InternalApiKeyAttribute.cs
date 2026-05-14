using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shop.Shared.Filters;

public class InternalApiKeyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var config = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        var expectedKey = config["InternalApiKey"];

        context.HttpContext.Request.Headers.TryGetValue("X-Internal-Key", out var key);

        if (key != expectedKey)
            context.Result = new UnauthorizedResult();
    }
}


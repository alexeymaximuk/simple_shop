using Microsoft.AspNetCore.Mvc;

namespace Shop.Users.Controllers; 

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
    
    [HttpPost]
    public IActionResult Create() => Ok();
}

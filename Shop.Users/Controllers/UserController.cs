using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Users.Controllers; 

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto, IValidator<CreateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        await _userService.CreateUser(dto);
        
        return Ok();
    }
}

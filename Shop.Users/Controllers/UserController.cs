using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shop.Users.DTOs;
using Shop.Users.Services.Interfaces;

namespace Shop.Users.Controllers; 

[ApiController]
[Route("api/users")]
public class UserController (IUserService userService, IAuthService authService) : ControllerBase
{
    /// <summary>
    /// POST api/users/register
    /// creates new user account and adds it to the database
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto dto, IValidator<CreateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        await userService.RegisterAsync(dto);
        
        return Ok();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [HttpGet("{guid}")]
    public async Task<IActionResult> GetById(Guid guid)
    {
        var user = await userService.GetByIdAsync(guid);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }


    /// <summary>
    /// Updates user username
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> EditUser(Guid id, UpdateUserDto dto, IValidator<UpdateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        await userService.UpdateAsync(id, dto);
        return Ok();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.DeleteAsync(id);
        return NoContent();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await userService.DeactivateAsync(id);
        return Ok();
    }


    [HttpGet("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var token = await authService.Login(dto);

        return Ok(token);
    }
}

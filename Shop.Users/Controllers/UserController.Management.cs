using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Users.DTOs;

namespace Shop.Users.Controllers;

public partial class UserController
{
    /// <summary>
    /// DELETE api/users/{id} — permanently removes user from database
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        await userService.DeleteSelf(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return NoContent();
    }
    
    /// <summary>
    /// DELETE api/users/{id} — permanently removes user from database
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.DeleteAsync(id);
        return NoContent();
    }
    
    /// <summary>
    /// POST api/users/{id}/deactivate — deactivates user, hides their products
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await userService.DeactivateAsync(id);
        return Ok();
    }
    
    /// <summary>
    /// POST api/users/{id}/activate — reactivates previously deactivated user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await userService.ActivateAsync(id);
        return Ok();
    }
    
    /// <summary>
    /// PATCH api/users/me/change-name — updates display name of existing user
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPatch("me/change-name")]
    public async Task<IActionResult> EditUserUsername(UpdateUsernameDto dto, IValidator<UpdateUsernameDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await userService.UpdateUsernameAsync(Guid.Parse(userId!), dto);
        return Ok();
    }
    
    /// <summary>
    /// GET api/users/{id} — returns user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    /// <summary>
    /// GET api/users — returns all users
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }
}
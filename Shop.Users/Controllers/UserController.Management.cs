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
    [Authorize]
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
    [Authorize]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await userService.ActivateAsync(id);
        return Ok();
    }
    
    /// <summary>
    /// PATCH api/users/{id}/change-name — updates display name of existing user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPatch("{id}/change-name")]
    public async Task<IActionResult> EditUserUsername(Guid id, UpdateUsernameDto dto, IValidator<UpdateUsernameDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        await userService.UpdateUsernameAsync(id, dto);
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
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }
}
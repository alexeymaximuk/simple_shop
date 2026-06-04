using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Shared.Controllers;
using Shop.Users.Application.DTOs.Users;
using Shop.Users.Application.Interfaces;

namespace Shop.Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UserManagementController(
    IUserService userService
) : ApiBaseController
{
    /// <summary>
    /// DELETE api/users/me — permanently removes the currently authenticated user from database
    /// </summary>
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        await userService.DeleteAsync(GetCurrentUserId());
        return NoContent();
    }

    /// <summary>
    /// DELETE api/users/{id} — permanently removes user from database (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await userService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// POST api/users/{id}/deactivate — deactivates user, hides their products (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await userService.DeactivateAsync(id);
        return Ok();
    }

    /// <summary>
    /// POST api/users/{id}/activate — reactivates previously deactivated user (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await userService.ActivateAsync(id);
        return Ok();
    }

    /// <summary>
    /// PATCH api/users/me/change-name — updates display name of currently authenticated user
    /// </summary>
    [Authorize]
    [HttpPatch("me/change-name")]
    public async Task<IActionResult> EditUserUsername(UpdateUsernameDto dto, IValidator<UpdateUsernameDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        await userService.UpdateUsernameAsync(GetCurrentUserId(), dto);
        return Ok();
    }

    /// <summary>
    /// GET api/users/{id} — returns user by id
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// GET api/users — returns all users (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }
}


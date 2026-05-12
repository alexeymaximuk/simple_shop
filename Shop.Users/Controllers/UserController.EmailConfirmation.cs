using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.Controllers;

public partial class UserController
{
    /// <summary>
    /// GET api/users/confirm-email — confirms user email using token sent to their inbox
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        await authService.ConfirmEmailAsync(token);

        return Ok();
    }

    /// <summary>
    /// POST api/users/resend-confirmation — resends email confirmation link to provided address
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] string email)
    {
        await authService.ResendConfirmationAsync(email);

        return Ok();
    }
    
    /// <summary>
    /// PATCH api/users/me/change-email — sends confirmation link to new email address
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("me/change-email")]
    public async Task<IActionResult> EditUserEmail(ChangeEmailRequestDto dto, IValidator<ChangeEmailRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await authService.ChangeEmailRequestAsync(Guid.Parse(userId!), dto);
        return Ok();
    }
    
    /// <summary>
    /// GET api/users/confirm-email-change — confirms email change using token sent to new address
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
    {
        await authService.ChangeEmailConfirmAsync(token);
        return Ok();
    }
}
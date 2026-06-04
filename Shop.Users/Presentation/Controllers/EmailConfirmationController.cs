using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Shared.Controllers;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;

namespace Shop.Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class EmailConfirmationController(
    IEmailVerificationService emailVerificationService
) : ApiBaseController
{
    /// <summary>
    /// GET api/users/confirm-email — confirms user email using token sent to their inbox
    /// </summary>
    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        await emailVerificationService.ConfirmEmailAsync(token);
        return Ok();
    }

    /// <summary>
    /// POST api/users/resend-confirmation — resends email confirmation link to provided address
    /// </summary>
    [AllowAnonymous]
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] string email)
    {
        await emailVerificationService.ResendConfirmationAsync(email);
        return Ok();
    }

    /// <summary>
    /// POST api/users/me/change-email — sends confirmation link to new email address
    /// </summary>
    [Authorize]
    [HttpPost("me/change-email")]
    public async Task<IActionResult> EditUserEmail(ChangeEmailRequestDto dto, IValidator<ChangeEmailRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        await emailVerificationService.ChangeEmailRequestAsync(GetCurrentUserId(), dto);
        return Ok();
    }

    /// <summary>
    /// GET api/users/confirm-email-change — confirms email change using token sent to new address
    /// </summary>
    [AllowAnonymous]
    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
    {
        await emailVerificationService.ChangeEmailConfirmAsync(token);
        return Ok();
    }
}


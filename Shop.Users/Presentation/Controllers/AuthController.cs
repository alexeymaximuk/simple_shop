using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;

namespace Shop.Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class AuthController(
    ILoginService loginService,
    IRegistrationService registrationService,
    IPasswordService passwordService
) : ControllerBase
{
    /// <summary>
    /// POST api/users/register — creates new user account and sends confirmation email
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto, IValidator<RegisterUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        await registrationService.RegisterAsync(dto);

        return Ok();
    }

    /// <summary>
    /// POST api/users/login — authenticates user and returns JWT token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto, IValidator<LoginUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var token = await loginService.Login(dto);

        return Ok(token);
    }

    /// <summary>
    /// POST api/users/reset-password-request — sends password reset link to provided email
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password-request")]
    public async Task<IActionResult> ResetPasswordRequest(ResetPasswordRequestDto dto, IValidator<ResetPasswordRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        await passwordService.ChangePasswordRequestAsync(dto);

        return Ok();
    }

    /// <summary>
    /// GET api/users/reset-password — validates password reset token from email link
    /// </summary>
    [AllowAnonymous]
    [HttpGet("reset-password")]
    public async Task<IActionResult> ResetPassword([FromQuery] string token)
    {
        await passwordService.ValidateChangePasswordRequestAsync(token);
        return Ok();
    }

    /// <summary>
    /// POST api/users/reset-password — applies new password using valid reset token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, IValidator<ResetPasswordDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);
        await passwordService.ChangePasswordAsync(dto);
        return Ok();
    }
}


using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.Controllers;

public partial class UserController
{
    /// <summary>
    /// POST api/users/register — creates new user account and sends confirmation email
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto, IValidator<RegisterUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        await authService.RegisterAsync(dto);

        return Ok();
    }
    
    /// <summary>
    /// POST api/users/login — authenticates user and returns JWT token
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto, IValidator<LoginUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);
        
        var token = await authService.Login(dto);

        return Ok(token);
    }
    
    /// <summary>
    /// POST api/users/reset-password-request — sends password reset link to provided email
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    [HttpPost("reset-password-request")]
    public async Task<IActionResult> ResetPasswordRequest(ResetPasswordRequestDto dto, IValidator<ResetPasswordRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        await authService.ChangePasswordRequestAsync(dto);

        return Ok();
    }

    /// <summary>
    /// GET api/users/reset-password — validates password reset token from email link
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("reset-password")]
    public async Task<IActionResult> ResetPassword([FromQuery] string token)
    {
        await authService.ValidateChangePasswordRequestAsync(token);
        return Ok();
    }
    
    /// <summary>
    /// POST api/users/reset-password — applies new password using valid reset token
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await authService.ChangePasswordAsync(dto);
        return Ok();
    }
}
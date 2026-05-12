using Shop.Users.DTOs.Auth;

namespace Shop.Users.Services.Interfaces;

public interface IAuthService
{
    // login
    Task<string> Login(LoginUserDto loginDto);
    
    // registration
    Task RegisterAsync(RegisterUserDto dto);

    
    // email confirmation
    Task ConfirmEmailAsync(string code);
    Task ChangeEmailRequestAsync(Guid id, ChangeEmailRequestDto dto);
    Task ChangeEmailConfirmAsync(string token);
    Task ResendConfirmationAsync(string email);

    // password reset
    Task ChangePasswordRequestAsync(ResetPasswordRequestDto dto);
    Task ValidateChangePasswordRequestAsync(string token);
    Task ChangePasswordAsync(ResetPasswordDto dto);
}
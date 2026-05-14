using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.Interfaces;

public interface IPasswordService
{
    Task ChangePasswordRequestAsync(ResetPasswordRequestDto dto);
    Task ValidateChangePasswordRequestAsync(string token);
    Task ChangePasswordAsync(ResetPasswordDto dto);
}
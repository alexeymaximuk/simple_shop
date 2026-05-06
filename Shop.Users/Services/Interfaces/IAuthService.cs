using Microsoft.IdentityModel.JsonWebTokens;
using Shop.Users.DTOs;

namespace Shop.Users.Services.Interfaces;

public interface IAuthService
{
    Task ResetPasswordAsync(Guid id);
    Task<string> Login(LoginUserDto loginDto);
}
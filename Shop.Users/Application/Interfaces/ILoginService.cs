using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.Interfaces;

public interface ILoginService
{
    Task<string> Login(LoginUserDto loginDto);
}
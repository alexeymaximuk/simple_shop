using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.Interfaces;

public interface IRegistrationService
{
    Task RegisterAsync(RegisterUserDto dto);
}
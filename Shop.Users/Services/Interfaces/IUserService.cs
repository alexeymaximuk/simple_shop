using Shop.Users.DTOs;
using Shop.Users.Models;

public interface IUserService
{
    Task<User> RegisterAsync(CreateUserDto dto);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task UpdateAsync(Guid id, UpdateUserDto dto);
    Task DeleteAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task ActivateAsync(Guid id);
}
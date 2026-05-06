using Shop.Users.DTOs;
using Shop.Users.Models;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task UpdateUsernameAsync(Guid id, UpdateUsernameDto dto);
    Task DeleteAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task ActivateAsync(Guid id);
}
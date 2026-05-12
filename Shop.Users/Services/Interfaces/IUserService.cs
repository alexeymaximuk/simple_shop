using Shop.Users.DTOs;

namespace Shop.Users.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task UpdateUsernameAsync(Guid id, UpdateUsernameDto dto);
    Task DeleteAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task ActivateAsync(Guid id);
    Task DeleteSelf(string? userId);
}
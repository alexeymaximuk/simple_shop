using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailConfirmationTokenAsync(string token);
    Task<User?> GetByEmailChangeTokenAsync(string token);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<IEnumerable<User>> GetAllAsync();
    void Add(User user);
    void Remove(User user);
    Task SaveChangesAsync();
}


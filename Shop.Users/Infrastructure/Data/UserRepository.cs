using Microsoft.EntityFrameworkCore;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;

namespace Shop.Users.Infrastructure.Data;

public class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id) =>
        await dbContext.Users.FindAsync(id);

    public Task<User?> GetByEmailAsync(string email) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> GetByEmailConfirmationTokenAsync(string token) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.EmailConfirmationToken == token);

    public Task<User?> GetByEmailChangeTokenAsync(string token) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.EmailChangeToken == token);

    public Task<User?> GetByPasswordResetTokenAsync(string token) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await dbContext.Users.ToListAsync();

    public void Add(User user) => dbContext.Users.Add(user);

    public void Remove(User user) => dbContext.Users.Remove(user);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}


using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Users.Data;
using Shop.Users.DTOs;
using Shop.Users.Exceptions;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class UserService (UsersDbContext dbContext, IPasswordHasher<User> passwordHasher)
    :  IUserService
{
    public async Task<User> RegisterAsync(CreateUserDto dto)
    {
        var existingUser = dbContext.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (existingUser != null) throw new DuplicateMailException("Mail already registered");
        
        var newUser = MapToEntity(dto);
        
        var hashedPassword = passwordHasher.HashPassword(newUser, dto.Password);
        newUser.PasswordHash = hashedPassword;
        
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();
        
        return newUser;
    }
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null) throw new NotFoundException($"User with id {id} not found");

        return user;
    }
    
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await dbContext.Users.ToListAsync();
    }
    
    public Task UpdateAsync(Guid id, UpdateUserDto dto)
    {
        throw new NotImplementedException();
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task DeactivateAsync(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        user.IsActive = false;
        await dbContext.SaveChangesAsync();
    }
    public async Task ActivateAsync(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        user.IsActive = false;
        await dbContext.SaveChangesAsync();
    }
}
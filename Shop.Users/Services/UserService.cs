using Microsoft.AspNetCore.Identity;
using Shop.Users.Data;
using Shop.Users.Models;

namespace Shop.Users.Services;

public class UserService :  IUserService
{
    private readonly UsersDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(UsersDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<User> CreateUser(CreateUserDto dto)
    {
        var newUser = MapToEntity(dto);
        
        var hashedPassword = _passwordHasher.HashPassword(newUser, dto.Password);
        newUser.PasswordHash = hashedPassword;
        
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();
        
        return newUser;
    }
    
    private static User MapToEntity(CreateUserDto dto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email
        };

        return user;
    }
}
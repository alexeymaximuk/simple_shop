using Microsoft.EntityFrameworkCore;
using Shop.Users.Data;
using Shop.Users.DTOs;
using Shop.Users.Exceptions;

namespace Shop.Users.Services;

public partial class UserService (UsersDbContext dbContext) : IUserService
{
    public async Task UpdateUsernameAsync(Guid id, UpdateUsernameDto dto)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        
        user.Name = dto.Name;
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
        user.IsActive = true;
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        return await dbContext.Users
            .Where(u => u.Id == id)
            .Select(MapToResponseDto)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        return await dbContext.Users
            .Select(MapToResponseDto)
            .ToListAsync();
    }
}
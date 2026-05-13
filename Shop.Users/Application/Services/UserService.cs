using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Users;
using Shop.Users.Application.Interfaces;

namespace Shop.Users.Application.Services;

public class UserService(
    IUserRepository userRepository, 
    IProductServiceClient productServiceClient
    ) : IUserService
{
    public async Task UpdateUsernameAsync(Guid id, UpdateUsernameDto dto)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");

        user.Name = dto.Name;
        await userRepository.SaveChangesAsync();
    }

    public async Task DeleteSelf(string? userIdString)
    {
        if (userIdString == null) throw new AuthorisationException("Cant get userId from current authorised user");
        var userId = Guid.Parse(userIdString);

        await DeleteAsync(userId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        userRepository.Remove(user);
        await userRepository.SaveChangesAsync();

        await productServiceClient.DeleteUserProducts(id);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        user.IsActive = false;
        await userRepository.SaveChangesAsync();

        await productServiceClient.DeactivateUserProducts(id);
    }

    public async Task ActivateAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null) throw new NotFoundException($"User {id} not found");
        user.IsActive = true;
        await userRepository.SaveChangesAsync();

        await productServiceClient.ReactivateUserProducts(id);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user == null ? null : MapToResponseDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(MapToResponseDto);
    }

    private static UserResponseDto MapToResponseDto(Domain.Models.User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role,
        IsActive = u.IsActive,
        IsEmailConfirmed = u.IsEmailConfirmed,
        CreatedAt = u.CreatedAt
    };
}
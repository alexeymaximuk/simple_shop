using Shop.Users.DTOs;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class UserService
{
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
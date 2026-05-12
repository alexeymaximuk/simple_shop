using System.Linq.Expressions;
using Shop.Users.DTOs;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class UserService
{
    private static Expression<Func<User,UserResponseDto>> MapToResponseDto => 
        u => new UserResponseDto
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
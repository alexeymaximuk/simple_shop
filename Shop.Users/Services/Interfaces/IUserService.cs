using Shop.Users.Models;

public interface IUserService
{
    public Task<User> CreateUser(CreateUserDto dto);
}
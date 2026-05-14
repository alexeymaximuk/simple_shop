namespace Shop.Users.Application.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
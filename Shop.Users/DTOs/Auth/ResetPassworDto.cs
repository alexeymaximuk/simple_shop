namespace Shop.Users.DTOs.Auth;

public class ResetPasswordDto
{
    public string Token { get; set; }
    public string Password { get; set; }
}
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Data;

namespace Shop.Users.Tests.Integration;

public class AuthControllerTests(UsersApiFactory factory) : IClassFixture<UsersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ResetDb()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
    }

    private User GetUser(string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        return db.Users.First(u => u.Email == email);
    }

    private void UpdateUser(string email, Action<User> update)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var user = db.Users.First(u => u.Email == email);
        update(user);
        db.SaveChanges();
    }

    private async Task RegisterUser(string email = "test@test.com", string name = "TestUser", string password = "Password123!")
    {
        await _client.PostAsJsonAsync("/api/users/register", new RegisterUserDto
        {
            Email = email,
            Name = name,
            Password = password
        });
    }

    private async Task RegisterAndConfirm(string email = "test@test.com")
    {
        await RegisterUser(email);
        var token = GetUser(email).EmailConfirmationToken!;
        await _client.GetAsync($"/api/users/confirm-email?token={token}");
    }

    [Fact]
    public async Task Register_ValidData_Returns200()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/register", new RegisterUserDto
        {
            Name = "Test",
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/register", new RegisterUserDto
        {
            Name = "Test",
            Email = "notanemail",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        ResetDb();
        var dto = new RegisterUserDto { Name = "Test", Email = "test@test.com", Password = "Password123!" };

        await _client.PostAsJsonAsync("/api/users/register", dto);
        var response = await _client.PostAsJsonAsync("/api/users/register", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        ResetDb();
        await RegisterAndConfirm();

        var response = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnconfirmedEmail_Returns401()
    {
        ResetDb();
        await RegisterUser();

        var response = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InactiveAccount_Returns403()
    {
        ResetDb();
        await RegisterAndConfirm();
        UpdateUser("test@test.com", u => u.IsActive = false);

        var response = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        ResetDb();
        await RegisterAndConfirm();

        var response = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = "test@test.com",
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns404()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResetPasswordRequest_RegisteredEmail_Returns200()
    {
        ResetDb();
        await RegisterAndConfirm();

        var response = await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "test@test.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPasswordRequest_NotRegistered_Returns404()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "nobody@test.com" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ValidateResetToken_ValidToken_Returns200()
    {
        ResetDb();
        await RegisterAndConfirm();
        await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "test@test.com" });

        var token = GetUser("test@test.com").PasswordResetToken!;
        var response = await _client.GetAsync($"/api/users/reset-password?token={token}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ValidateResetToken_InvalidToken_Returns404()
    {
        ResetDb();
        var response = await _client.GetAsync("/api/users/reset-password?token=invalid-token");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ValidateResetToken_ExpiredToken_Returns400()
    {
        ResetDb();
        await RegisterAndConfirm();
        await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "test@test.com" });

        UpdateUser("test@test.com", u => u.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1));

        var token = GetUser("test@test.com").PasswordResetToken!;
        var response = await _client.GetAsync($"/api/users/reset-password?token={token}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_Returns200()
    {
        ResetDb();
        await RegisterAndConfirm();
        await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "test@test.com" });

        var token = GetUser("test@test.com").PasswordResetToken!;
        var response = await _client.PostAsJsonAsync("/api/users/reset-password", new ResetPasswordDto
        {
            Token = token,
            Password = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_Returns404()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/reset-password", new ResetPasswordDto
        {
            Token = "invalid-token",
            Password = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_Returns400()
    {
        ResetDb();
        await RegisterAndConfirm();
        await _client.PostAsJsonAsync("/api/users/reset-password-request",
            new ResetPasswordRequestDto { Email = "test@test.com" });

        UpdateUser("test@test.com", u => u.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1));

        var token = GetUser("test@test.com").PasswordResetToken!;
        var response = await _client.PostAsJsonAsync("/api/users/reset-password", new ResetPasswordDto
        {
            Token = token,
            Password = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
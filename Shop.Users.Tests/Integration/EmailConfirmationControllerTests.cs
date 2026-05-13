using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Data;

namespace Shop.Users.Tests.Integration;

public class EmailConfirmationControllerTests(UsersApiFactory factory) : IClassFixture<UsersApiFactory>
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

    private async Task<string> RegisterConfirmAndLogin(string email = "test@test.com", string name = "TestUser", string password = "Password123!")
    {
        await RegisterUser(email, name, password);

        var token = GetUser(email).EmailConfirmationToken!;
        await _client.GetAsync($"/api/users/confirm-email?token={token}");

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = email,
            Password = password
        });

        var raw = await loginResponse.Content.ReadAsStringAsync();
        return raw.Trim('"');
    }

    [Fact]
    public async Task ConfirmEmail_ValidToken_Returns200()
    {
        ResetDb();
        await RegisterUser();

        var token = GetUser("test@test.com").EmailConfirmationToken!;
        var response = await _client.GetAsync($"/api/users/confirm-email?token={token}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_InvalidToken_Returns404()
    {
        ResetDb();
        var response = await _client.GetAsync("/api/users/confirm-email?token=invalid-token");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_ExpiredToken_Returns400()
    {
        ResetDb();
        await RegisterUser();

        UpdateUser("test@test.com", u => u.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(-1));

        var token = GetUser("test@test.com").EmailConfirmationToken!;
        var response = await _client.GetAsync($"/api/users/confirm-email?token={token}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResendConfirmation_UnconfirmedEmail_Returns200()
    {
        ResetDb();
        await RegisterUser();

        var response = await _client.PostAsJsonAsync("/api/users/resend-confirmation", "test@test.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResendConfirmation_EmailNotRegistered_Returns404()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/resend-confirmation", "nobody@test.com");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResendConfirmation_AlreadyConfirmed_Returns422()
    {
        ResetDb();
        await RegisterUser();

        var token = GetUser("test@test.com").EmailConfirmationToken!;
        await _client.GetAsync($"/api/users/confirm-email?token={token}");

        var response = await _client.PostAsJsonAsync("/api/users/resend-confirmation", "test@test.com");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_ValidRequest_Returns200()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.PostAsJsonAsync("/api/users/me/change-email", new ChangeEmailRequestDto
        {
            NewEmail = "newemail@test.com"
        });
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.PostAsJsonAsync("/api/users/me/change-email", new ChangeEmailRequestDto
        {
            NewEmail = "newemail@test.com"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_InvalidEmailFormat_Returns400()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.PostAsJsonAsync("/api/users/me/change-email", new ChangeEmailRequestDto
        {
            NewEmail = "not-an-email"
        });
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmailChange_ValidToken_Returns200()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        await _client.PostAsJsonAsync("/api/users/me/change-email", new ChangeEmailRequestDto
        {
            NewEmail = "newemail@test.com"
        });
        _client.DefaultRequestHeaders.Authorization = null;

        var token = GetUser("test@test.com").EmailChangeToken!;
        var response = await _client.GetAsync($"/api/users/confirm-email-change?token={token}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmailChange_InvalidToken_Returns404()
    {
        ResetDb();
        var response = await _client.GetAsync("/api/users/confirm-email-change?token=invalid-token");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmailChange_ExpiredToken_Returns400()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        await _client.PostAsJsonAsync("/api/users/me/change-email", new ChangeEmailRequestDto
        {
            NewEmail = "newemail@test.com"
        });
        _client.DefaultRequestHeaders.Authorization = null;

        UpdateUser("test@test.com", u => u.EmailChangeTokenExpiry = DateTime.UtcNow.AddHours(-1));

        var token = GetUser("test@test.com").EmailChangeToken!;
        var response = await _client.GetAsync($"/api/users/confirm-email-change?token={token}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
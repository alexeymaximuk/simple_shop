using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.DTOs.Users;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Data;

namespace Shop.Users.Tests.Integration;

public class UserManagementControllerTests(UsersApiFactory factory) : IClassFixture<UsersApiFactory>
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

    private async Task RegisterUser(string email, string name = "TestUser", string password = "Password123!")
    {
        await _client.PostAsJsonAsync("/api/users/register", new RegisterUserDto
        {
            Email = email,
            Name = name,
            Password = password
        });
    }

    private async Task<string> RegisterConfirmAndLogin(string email, string name = "TestUser", string password = "Password123!")
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

    private async Task<string> RegisterConfirmAndLoginAsAdmin(string email, string name = "AdminUser", string password = "Password123!")
    {
        await RegisterUser(email, name, password);

        UpdateUser(email, u =>
        {
            u.Role = "Admin";
            u.IsEmailConfirmed = true;
            u.EmailConfirmationToken = null;
            u.EmailConfirmationTokenExpiry = null;
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new LoginUserDto
        {
            Email = email,
            Password = password
        });

        var raw = await loginResponse.Content.ReadAsStringAsync();
        return raw.Trim('"');
    }

    private void SetAuth(string jwt) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    private void ClearAuth() =>
        _client.DefaultRequestHeaders.Authorization = null;

    [Fact]
    public async Task DeleteCurrentUser_Authenticated_Returns204()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin("user@test.com");

        SetAuth(jwt);
        var response = await _client.DeleteAsync("/api/users/me");
        ClearAuth();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCurrentUser_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.DeleteAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_Returns204()
    {
        ResetDb();
        var adminJwt = await RegisterConfirmAndLoginAsAdmin("admin@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        var targetId = GetUser("target@test.com").Id;

        SetAuth(adminJwt);
        var response = await _client.DeleteAsync($"/api/users/{targetId}");
        ClearAuth();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_AsRegularUser_Returns403()
    {
        ResetDb();
        var userJwt = await RegisterConfirmAndLogin("user@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        var targetId = GetUser("target@test.com").Id;

        SetAuth(userJwt);
        var response = await _client.DeleteAsync($"/api/users/{targetId}");
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_NotFound_Returns404()
    {
        ResetDb();
        var adminJwt = await RegisterConfirmAndLoginAsAdmin("admin@test.com");

        SetAuth(adminJwt);
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");
        ClearAuth();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_AsAdmin_Returns200()
    {
        ResetDb();
        var adminJwt = await RegisterConfirmAndLoginAsAdmin("admin@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        var targetId = GetUser("target@test.com").Id;

        SetAuth(adminJwt);
        var response = await _client.PostAsync($"/api/users/{targetId}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_AsRegularUser_Returns403()
    {
        ResetDb();
        var userJwt = await RegisterConfirmAndLogin("user@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        var targetId = GetUser("target@test.com").Id;

        SetAuth(userJwt);
        var response = await _client.PostAsync($"/api/users/{targetId}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.PostAsync($"/api/users/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ActivateUser_AsAdmin_Returns200()
    {
        ResetDb();
        var adminJwt = await RegisterConfirmAndLoginAsAdmin("admin@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        UpdateUser("target@test.com", u => u.IsActive = false);
        var targetId = GetUser("target@test.com").Id;

        SetAuth(adminJwt);
        var response = await _client.PostAsync($"/api/users/{targetId}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ActivateUser_AsRegularUser_Returns403()
    {
        ResetDb();
        var userJwt = await RegisterConfirmAndLogin("user@test.com");
        await RegisterConfirmAndLogin("target@test.com");
        var targetId = GetUser("target@test.com").Id;

        SetAuth(userJwt);
        var response = await _client.PostAsync($"/api/users/{targetId}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_ValidData_Returns200()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin("user@test.com");

        SetAuth(jwt);
        var response = await _client.PatchAsJsonAsync("/api/users/me/change-name", new UpdateUsernameDto
        {
            Name = "NewName"
        });
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.PatchAsJsonAsync("/api/users/me/change-name", new UpdateUsernameDto
        {
            Name = "NewName"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_TooShort_Returns400()
    {
        ResetDb();
        var jwt = await RegisterConfirmAndLogin("user@test.com");

        SetAuth(jwt);
        var response = await _client.PatchAsJsonAsync("/api/users/me/change-name", new UpdateUsernameDto
        {
            Name = "AB"
        });
        ClearAuth();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingUser_Returns200()
    {
        ResetDb();
        await RegisterConfirmAndLogin("user@test.com");
        var userId = GetUser("user@test.com").Id;

        var response = await _client.GetAsync($"/api/users/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        ResetDb();
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_Returns200()
    {
        ResetDb();
        var adminJwt = await RegisterConfirmAndLoginAsAdmin("admin@test.com");

        SetAuth(adminJwt);
        var response = await _client.GetAsync("/api/users");
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsRegularUser_Returns403()
    {
        ResetDb();
        var userJwt = await RegisterConfirmAndLogin("user@test.com");

        SetAuth(userJwt);
        var response = await _client.GetAsync("/api/users");
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        ResetDb();
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}


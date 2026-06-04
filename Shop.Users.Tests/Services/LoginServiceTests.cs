using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shop.Shared.Exceptions;
using Shop.Shared.Interfaces;
using Shop.Shared.Settings;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Domain.Models;

namespace Shop.Users.Tests.Services;

public class LoginServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRedisSessionService _redisSessionService = Substitute.For<IRedisSessionService>();
    private readonly IPasswordHasher<User> _passwordHasher = Substitute.For<IPasswordHasher<User>>();
    private readonly JwtSettings _jwtSettings = new()
    {
        Key = "test-secret-key-minimum-32-characters",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        TokenDuration = 1
    };

    private readonly LoginService _sut;

    public LoginServiceTests()
    {
        _sut = new LoginService(_userRepository, _passwordHasher, _jwtSettings, _redisSessionService);
    }

    #region Login

    [Fact]
    public async Task Login_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Login(new LoginUserDto { Email = "test@test.com", Password = "password" }));
    }

    [Fact]
    public async Task Login_UserNotActive_ThrowsAccountDeactivatedException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User { IsActive = false, Name = "Test", Email = "test@test.com", PasswordHash = "hash" });

        await Assert.ThrowsAsync<AccountDeactivatedException>(() =>
            _sut.Login(new LoginUserDto { Email = "test@test.com", Password = "password" }));
    }

    [Fact]
    public async Task Login_EmailNotConfirmed_ThrowsAuthorisationException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User
        {
            IsActive = true,
            IsEmailConfirmed = false,
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await Assert.ThrowsAsync<AuthorisationException>(() =>
            _sut.Login(new LoginUserDto { Email = "test@test.com", Password = "password" }));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsAuthorisationException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User
        {
            IsActive = true,
            IsEmailConfirmed = true,
            PasswordHash = "hash",
            Name = "Test",
            Email = "test@test.com"
        });

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<AuthorisationException>(() =>
            _sut.Login(new LoginUserDto { Email = "test@test.com", Password = "wrongpassword" }));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            Name = "Test",
            Role = "User",
            IsActive = true,
            IsEmailConfirmed = true,
            PasswordHash = "hash"
        });

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(PasswordVerificationResult.Success);

        var result = await _sut.Login(new LoginUserDto { Email = "test@test.com", Password = "password" });

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion
}
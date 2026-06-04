using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Domain.Models;

namespace Shop.Users.Tests.Services;

public class PasswordServiceTest
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IEmailSendingService _emailSendingService = Substitute.For<IEmailSendingService>();
    private readonly IPasswordHasher<User> _passwordHasher =  Substitute.For<IPasswordHasher<User>>();
    
    private readonly PasswordService _sut;

    public PasswordServiceTest()
    {
        _sut = new PasswordService(_userRepository, _emailSendingService, _passwordHasher);
    }

    #region ChangePasswordRequestAsync

    [Fact]
    public async Task ChangePasswordRequestAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangePasswordRequestAsync(new ResetPasswordRequestDto {Email = "test@test.com"})
        );
    }

    [Fact]
    public async Task ChangePasswordRequestAsync_UserDeactivated_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User { IsActive = false, Name = "Test", Email = "test@test.com", PasswordHash = "hash" });

        await Assert.ThrowsAsync<AccountDeactivatedException>(() =>
            _sut.ChangePasswordRequestAsync(new ResetPasswordRequestDto {Email = "test@test.com"})
        );
    }
    
    [Fact]
    public async Task ChangePasswordRequestAsync_ValidRequest_SendsPasswordResetMail()
    {
        var user = new User { Email = "test@test.com", Name = "Test", PasswordHash = "hash" };
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        await _sut.ChangePasswordRequestAsync(new ResetPasswordRequestDto {Email = user.Email});

        await _emailSendingService.Received(1).SendPasswordResetMailAsync(user.Email, Arg.Any<string>());
    }
    
    [Fact]
    public async Task ChangePasswordRequestAsync_ValidUser_SetsTokenAndExpiry()
    {
        var user = new User { IsActive = true, Email = "test@test.com", Name = "Test", PasswordHash = "hash" };
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        await _sut.ChangePasswordRequestAsync(new ResetPasswordRequestDto { Email = user.Email });

        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiry);
        Assert.True(user.PasswordResetTokenExpiry > DateTime.UtcNow);
    }

    #endregion


    #region ValidateChangePasswordRequestAsync

    [Fact]
    public async Task ValidateChangePasswordRequestAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns((User?) null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ValidateChangePasswordRequestAsync("test")
        );
    }
    
    [Fact]
    public async Task ValidateChangePasswordRequestAsync_TokenExpired_ThrowsTokenExpiredException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(new User {
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1),
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ValidateChangePasswordRequestAsync("test")
        );
    }
    
    [Fact]
    public async Task ValidateChangePasswordRequestAsync_TokenExpiryNull_ThrowsTokenExpiredException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(new User {
            PasswordResetTokenExpiry = null,
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ValidateChangePasswordRequestAsync("test")
        );
    }

    [Fact]
    public async Task ValidateChangePasswordRequestAsync_ValidToken_DoesNotThrow()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(new User
        {
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1),
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        var exception = await Record.ExceptionAsync(() => _sut.ValidateChangePasswordRequestAsync("valid-token"));

        Assert.Null(exception);
    }

    #endregion


    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns((User?) null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangePasswordAsync(new ResetPasswordDto {Token = "test", Password = "newpassword"})
        );
    }
    
    [Fact]
    public async Task ChangePasswordAsync_TokenExpired_ThrowsTokenExpiredException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(new User {
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1),
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ChangePasswordAsync(new ResetPasswordDto {Token = "test", Password = "newpassword"})
        );
    }
    
    [Fact]
    public async Task ChangePasswordAsync_TokenExpiryNull_ThrowsTokenExpiredException()
    {
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(new User {
            PasswordResetTokenExpiry = null,
            Name = "Test",
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ChangePasswordAsync(new ResetPasswordDto {Token = "test", Password = "newpassword"})
        );
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ValidToken_HashesPasswordAndSendsEmail()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1),
            Name = "Test",
            PasswordHash = "hash"
        };
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns("newhash");

        await _sut.ChangePasswordAsync(new ResetPasswordDto { Token = "token", Password = "newpass" });

        Assert.Equal("newhash", user.PasswordHash);
        await _emailSendingService.Received(1).SendYourPasswordWasRecentlyChanged(user.Email);
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ValidToken_ClearsResetToken()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordResetToken = "token",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1),
            Name = "Test",
            PasswordHash = "hash"
        };
        _userRepository.GetByPasswordResetTokenAsync(Arg.Any<string>()).Returns(user);

        await _sut.ChangePasswordAsync(new ResetPasswordDto { Token = "token", Password = "newpass" });

        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetTokenExpiry);
    }

    #endregion
}
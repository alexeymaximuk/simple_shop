using NSubstitute;
using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Email;

namespace Shop.Users.Tests.Services;

public class EmailVerificationServiceTest
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IEmailSendingService _emailSendingService = Substitute.For<IEmailSendingService>();

    private readonly EmailVerificationService _sut;

    public EmailVerificationServiceTest()
    {
        _sut = new EmailVerificationService(_userRepository, _emailSendingService);
    }

    #region ChangeEmailRequestAsync

    [Fact]
    public async Task ChangeEmailRequestAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ChangeEmailRequestAsync(
            Guid.NewGuid(), new ChangeEmailRequestDto { NewEmail = "test@test.com"}
        ));
    }
    
    
    [Fact]
    public async Task ChangeEmailRequestAsync_ValidData_SendsConfirmationMail()
    {
        var user = new User { Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);

        await  _sut.ChangeEmailRequestAsync(
            Guid.NewGuid(), new ChangeEmailRequestDto { NewEmail = "test@test.com"}
        );
        
        Assert.NotNull(user.EmailChangeToken);
        Assert.NotNull(user.EmailChangeTokenExpiry);
        Assert.True(user.EmailChangeTokenExpiry > DateTime.UtcNow);
        Assert.Equal("test@test.com", user.PendingEmail);
        await _userRepository.Received(1).SaveChangesAsync();
        await _emailSendingService.Received(1).SendEmailChangeMailAsync(user.PendingEmail!, user.EmailChangeToken!);
    }

    #endregion
    

    #region ChangeEmailConfirmAsync

    [Fact]
    public async Task ChangeEmailConfirmAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailChangeTokenAsync(Arg.Any<string>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _sut.ChangeEmailConfirmAsync(Arg.Any<string>()));
    }

    [Fact]
    public async Task ChangeEmailConfirmAsync_PendingEmailEmpty_ThrowsInvalidRequestException()
    {
        var user = new User { PendingEmail = null, Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailChangeTokenAsync(Arg.Any<string>()).Returns(user);
        
        await Assert.ThrowsAsync<InvalidRequestException>(() => 
            _sut.ChangeEmailConfirmAsync("token"));
    }
    
    [Fact]
    public async Task ChangeEmailConfirmAsync_TokenExpiryNull_ThrowsTokenExpiredException()
    {
        var user = new User { PendingEmail = "newemail@test.com", EmailChangeTokenExpiry = null, Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailChangeTokenAsync(Arg.Any<string>()).Returns(user);
        
        await Assert.ThrowsAsync<TokenExpiredException>(() => 
            _sut.ChangeEmailConfirmAsync("token"));
    }

    [Fact]
    public async Task ChangeEmailConfirmAsync_ExpiredToken_ThrowsTokenExpiredException()
    {
        var user = new User { PendingEmail = "newemail@test.com", EmailChangeTokenExpiry = DateTime.UtcNow.AddHours(-1), Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailChangeTokenAsync(Arg.Any<string>()).Returns(user);
        
        await Assert.ThrowsAsync<TokenExpiredException>(() => 
            _sut.ChangeEmailConfirmAsync("token"));
    }
    
    [Fact]
    public async Task ChangeEmailConfirmAsync_ValidData_ChangesPasswordAndClearsFields()
    {
        var user = new User { 
            Email = "oldemail@test.com",
            EmailChangeToken = "testtoken",
            PendingEmail = "newemail@test.com", 
            EmailChangeTokenExpiry = DateTime.UtcNow.AddHours(1),
            Name = "Test",
            PasswordHash = "hash"
        };
        
        _userRepository.GetByEmailChangeTokenAsync(Arg.Any<string>()).Returns(user);
        
        await _sut.ChangeEmailConfirmAsync(user.EmailChangeToken);
        Assert.Equal("newemail@test.com", user.Email);
        Assert.Null(user.PendingEmail);
        Assert.Null(user.EmailChangeToken);
        Assert.Null(user.EmailChangeTokenExpiry);
        Assert.True(user.IsEmailConfirmed);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    #endregion


    #region ConfirmEmailAsync

    [Fact]
    public async Task ConfirmEmailAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailConfirmationTokenAsync(Arg.Any<string>()).Returns((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ConfirmEmailAsync("token"));
    }
    
    [Fact]
    public async Task ConfirmEmailAsync_TokenExpiryNull_ThrowsTokenExpiredException()
    {
        var user =  new User { EmailConfirmationTokenExpiry = null, Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailConfirmationTokenAsync(Arg.Any<string>()).Returns(user);
        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ConfirmEmailAsync("token"));
    }

    [Fact]
    public async Task ConfirmEmailAsync_TokenExpired_ThrowsTokenExpiredException()
    {
        var user =  new User { EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(-1), Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailConfirmationTokenAsync(Arg.Any<string>()).Returns(user);
        await Assert.ThrowsAsync<TokenExpiredException>(() =>
            _sut.ConfirmEmailAsync("token"));
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidData_ConfirmsUserEmail()
    {
        var user = new User { IsEmailConfirmed = false, EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(1), Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailConfirmationTokenAsync(Arg.Any<string>()).Returns(user);
        
        await _sut.ConfirmEmailAsync("token");
        
        Assert.True(user.IsEmailConfirmed);
        Assert.Null(user.EmailConfirmationToken);
        Assert.Null(user.EmailConfirmationTokenExpiry);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    #endregion
    

    #region ResendConfirmationAsync

    [Fact]
    public async Task ResendConfirmationAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ResendConfirmationAsync("test@test.com"));
    }
    
    [Fact]
    public async Task ResendConfirmationAsync_EmailConfirmed_ThrowsInvalidRequestException()
    {
        var user = new User { IsEmailConfirmed = true, Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            _sut.ResendConfirmationAsync("test@test.com"));
    }

    [Fact]
    public async Task ResendConfirmationAsync_ValidData_SendsTokenInEmail()
    {
        var user = new User { IsEmailConfirmed = false, EmailConfirmationToken = "oldtoken", Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        
        await _sut.ResendConfirmationAsync(user.Email);
        await _userRepository.Received(1).SaveChangesAsync();
        
        Assert.NotNull(user.EmailConfirmationToken);
        Assert.NotEqual("oldtoken", user.EmailConfirmationToken);
        Assert.NotNull(user.EmailConfirmationTokenExpiry);
        Assert.True(user.EmailConfirmationTokenExpiry > DateTime.UtcNow);
        await _emailSendingService.Received(1).SendEmailConfirmationMailAsync(user.Email, user.EmailConfirmationToken!);
    }

    #endregion
}
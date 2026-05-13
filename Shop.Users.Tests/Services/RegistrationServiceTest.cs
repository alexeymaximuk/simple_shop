using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Domain.Models;

namespace Shop.Users.Tests.Services;

public class RegistrationServiceTest
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher<User> _passwordHasher = Substitute.For<IPasswordHasher<User>>();
    private readonly IEmailVerificationService _emailVerificationService = Substitute.For<IEmailVerificationService>();
    
    private readonly RegistrationService _sut;

    public RegistrationServiceTest()
    {
        _sut = new RegistrationService(_userRepository, _passwordHasher, _emailVerificationService);
    }

    [Fact]
    public async Task RegisterAsync_UserExists_ThrowsDuplicateMailException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User());

        await Assert.ThrowsAsync<DuplicateMailException>(() => _sut.RegisterAsync(
                new RegisterUserDto {Name = "Test", Email = "test@test.com", Password = "password"}
            )
        );
    }

    [Fact]
    public async Task RegisterAsync_ValidCredentials_Success()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        var dto = new RegisterUserDto {Name = "Test", Email = "test@test.com", Password = "password"};
        await _sut.RegisterAsync(dto);
        await _emailVerificationService.Received(1)
            .SendEmailConfirmationAsync(Arg.Is<User>(x => x.Email == dto.Email && x.Name == dto.Name));
    }
    
    [Fact]
    public async Task RegisterAsync_ValidCredentials_AddsUserToRepository()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns("hash");

        await _sut.RegisterAsync(
            new RegisterUserDto { Name = "Test", Email = "test@test.com", Password = "password" }
        );

        _userRepository.Received(1).Add(Arg.Is<User>(u => u.Email == "test@test.com"));
    }

    [Fact]
    public async Task RegisterAsync_ValidCredentials_HashesPassword()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>()).Returns("hash");

        await _sut.RegisterAsync(
            new RegisterUserDto { Name = "Test", Email = "test@test.com", Password = "password" }
        );

        _passwordHasher.Received(1).HashPassword(Arg.Any<User>(), "password");
    }
}
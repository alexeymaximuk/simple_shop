using Microsoft.AspNetCore.Identity;
using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Services;

public class RegistrationService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IEmailVerificationService emailVerificationService
    ) : IRegistrationService
{
    public async Task RegisterAsync(RegisterUserDto dto)
    {
        var existingUser = await userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null) throw new DuplicateMailException("Mail already registered");

        var newUser = MapToEntity(dto);

        newUser.PasswordHash = passwordHasher.HashPassword(newUser, dto.Password);

        userRepository.Add(newUser);
        await userRepository.SaveChangesAsync();
        await emailVerificationService.SendEmailConfirmationAsync(newUser);
    }

    private static User MapToEntity(RegisterUserDto dto) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email
        };
}
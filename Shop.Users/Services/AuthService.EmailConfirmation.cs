using Microsoft.EntityFrameworkCore;
using Shop.Shared.Exceptions;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class AuthService
{
    public async Task ConfirmEmailAsync(string emailConfirmationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.EmailConfirmationToken == emailConfirmationToken);
        if (user == null) throw new NotFoundException("Invalid token");
        if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");
        
        user.IsEmailConfirmed = true;
        
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        
        await dbContext.SaveChangesAsync();
    }

    public async Task ResendConfirmationAsync(string email)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null) throw new NotFoundException("User not found");
        if (user.IsEmailConfirmed) throw new InvalidRequestException("Email already confirmed");
        
        await SendEmailConfirmationAsync(user);
    }

    private async Task SendEmailConfirmationAsync(User user)
    {
        var expiry = DateTime.UtcNow.AddHours(AuthConstants.EmailConfirmationTokenExpiryHours);
        
        user.EmailConfirmationToken = GenerateToken();
        user.EmailConfirmationTokenExpiry = expiry;
        
        await dbContext.SaveChangesAsync();
        await emailService.SendEmailConfirmationMailAsync(user.Email, user.EmailConfirmationToken);
    }
}
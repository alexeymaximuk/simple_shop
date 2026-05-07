using Microsoft.EntityFrameworkCore;
using Shop.Shared.Exceptions;
using Shop.Users.DTOs.Auth;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class AuthService
{
    public async Task ChangeEmailRequestAsync(Guid id, ChangeEmailRequestDto dto)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null) throw new NotFoundException("User not found");
        
        user.PendingEmail = dto.NewEmail;
        
        await SendEmailChangeConfirmationAsync(user);
    }
    
    public async Task ChangeEmailConfirmAsync(string token)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.EmailChangeToken == token);
        if (user == null) throw new NotFoundException("User with that email change token not found");
        if (user.PendingEmail == null) throw new InvalidRequestException("User has not been pending email");
        if (user.EmailChangeTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");
        
        user.Email = user.PendingEmail;
        
        user.EmailChangeToken = null;
        user.EmailChangeTokenExpiry = null;
        user.IsEmailConfirmed = true;
        user.PendingEmail = null;
        
        await dbContext.SaveChangesAsync();
    }
    
    private async Task SendEmailChangeConfirmationAsync(User user)
    {
        user.EmailChangeToken = GenerateToken();
        user.EmailChangeTokenExpiry = DateTime.UtcNow.AddHours(AuthConstants.ChangeEmailConfirmationTokenExpiryHours);
        await dbContext.SaveChangesAsync();
        await emailService.SendEmailChangeMailAsync(user.PendingEmail!, user.EmailChangeToken);
    }
}
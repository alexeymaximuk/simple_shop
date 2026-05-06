using System.ComponentModel.DataAnnotations;

namespace Shop.Users.Models;

/// <summary>
/// Application user
/// </summary>
public class User
{
    /// <summary>
    /// User id
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Username
    /// </summary>
    [MaxLength(256)]
    public string Name { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    [MaxLength(256)]
    public string Email { get; set; }
    
    /// <summary>
    /// Password is hashed using PasswordHasher
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// For now there are only 2 possible roles, Default and Admin
    /// </summary>
    [MaxLength(256)]
    public string Role { get; set; } = "User";
    
    /// <summary>
    /// A way to mark account as inactive (and possibly delete later)
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Email confirmation
    /// <summary>
    /// Indicates whether the user's email has been confirmed.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }
    
    /// <summary>
    /// Token sent to user's email for account confirmation.
    /// </summary>
    [MaxLength(256)]
    public string? EmailConfirmationToken { get; set; }
    
    /// <summary>
    /// Expiry time for the email confirmation token.
    /// </summary>
    public DateTime? EmailConfirmationTokenExpiry { get; set; }

    // Email change
    /// <summary>
    /// New email address waiting to be confirmed.
    /// </summary>
    [MaxLength(256)]
    public string? PendingEmail { get; set; }
    
    /// <summary>
    /// Token sent to the new email address for email change confirmation.
    /// </summary>
    [MaxLength(256)]
    public string? EmailChangeToken { get; set; }
    
    /// <summary>
    /// Expiry time for the email change token.
    /// </summary>
    public DateTime? EmailChangeTokenExpiry { get; set; }

    // Password reset
    /// <summary>
    /// Token sent to user's email for password reset.
    /// </summary>
    [MaxLength(256)]
    public string? PasswordResetToken { get; set; }
    
    /// <summary>
    /// Expiry time for the password reset token.
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }
}
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
    public string Name { get; set; }
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Password is hashed using PasswordHasher
    /// </summary>
    public string PasswordHash { get; set; }
    /// <summary>
    /// For now there are only 2 possible roles, Default and Admin
    /// </summary>
    public string Role { get; set; }
    
    
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
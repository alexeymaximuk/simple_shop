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
    /// Password hash
    /// </summary>
    public string PasswordHash { get; set; }
}
using System.Security.Cryptography;

namespace Shop.Shared.Helpers;

/// <summary>
/// Provides cryptographically secure token generation utilities.
/// </summary>
public static class TokenGenerator
{
    /// <summary>
    /// Generates a URL-safe, cryptographically secure random token (86 chars, base64url encoded).
    /// </summary>
    public static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}


namespace Shop.Users.Services;

public static class AuthConstants
{
    public const string ConfirmEmailRoute = "api/users/confirm-email";
    public const string ConfirmEmailChangeRoute = "api/users/confirm-email-change";
    public const string ResetPasswordRoute = "api/users/reset-password";
    
    public const int ResetPasswordTokenExpiryHours = 5;
    public const int EmailConfirmationTokenExpiryHours = 24;
    public const int ChangeEmailConfirmationTokenExpiryHours = 24;
}
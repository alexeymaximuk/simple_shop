namespace Shop.Shared.Constants;

public static class AuthConstants
{
    public const string ConfirmEmailRoute = "Auth/ConfirmEmail";
    public const string ConfirmEmailChangeRoute = "Auth/ConfirmEmailChange";
    public const string ResetPasswordRoute = "Auth/ResetPassword";
    
    public const int ResetPasswordTokenExpiryHours = 5;
    public const int EmailConfirmationTokenExpiryHours = 24;
    public const int ChangeEmailConfirmationTokenExpiryHours = 24;
}
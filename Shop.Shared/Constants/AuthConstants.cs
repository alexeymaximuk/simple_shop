namespace Shop.Shared.Constants;

public static class AuthConstants
{
    public const string ConfirmEmailRoute = "Email/ConfirmEmail";
    public const string ConfirmEmailChangeRoute = "Email/ConfirmEmailChange";
    public const string ResetPasswordRoute = "Password/ResetPassword";
    
    public const int ResetPasswordTokenExpiryHours = 5;
    public const int EmailConfirmationTokenExpiryHours = 24;
    public const int ChangeEmailConfirmationTokenExpiryHours = 24;
}
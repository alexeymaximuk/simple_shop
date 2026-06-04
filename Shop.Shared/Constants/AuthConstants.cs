namespace Shop.Shared.Constants;

public static class AuthConstants
{
    public const string ConfirmEmailRoute = "Email/ConfirmEmail";
    public const string ConfirmEmailChangeRoute = "Email/ConfirmEmailChange";
    public const string ResetPasswordRoute = "Password/ResetPassword";

    public const string SessionKeyPrefix = "user-session-jti:";

    public const int ResetPasswordTokenExpiryHours = 5;
    public const int EmailConfirmationTokenExpiryHours = 24;
    public const int ChangeEmailConfirmationTokenExpiryHours = 24;

    public static class ClaimNames
    {
        public const string Sub = "sub";
        public const string Jti = "jti";
        public const string Name = "name";
        public const string Email = "email";
        public const string Role = "role";
    }
}
namespace Shop.Users.Constants;

public class EmailTemplates
{
    public const string ConfirmEmailSubject = "Confirm your email";
    public const string ResetPasswordSubject = "Reset your password";
    public const string ChangeEmailSubject = "Confirm email change";
    public const string PasswordWasRecentlyChangedSubject = "Password has been changed";
    
    public static string ConfirmEmail(string link) =>
        $"""
         <h1>Confirm your email</h1>
         <p>Click the link below to confirm your email address.</p>
         <a href='{link}'>Confirm Email</a>
         """;

    public static string ResetPassword(string link) =>
        $"""
         <h1>Reset your password</h1>
         <p>Click the link below to reset your password. Link expires in 300 minutes.</p>
         <a href='{link}'>Reset Password</a>
         """;

    public static string ChangeEmail(string link) =>
        $"""
         <h1>Confirm email change</h1>
         <p>Click the link below to confirm your new email address. Link expires in 1 hour.</p>
         <a href='{link}'>Confirm Email Change</a>
         """;

    public static string PasswordWasRecentlyChanged() => 
        """
        <h1>Password has been changed</h1>
        """;
}
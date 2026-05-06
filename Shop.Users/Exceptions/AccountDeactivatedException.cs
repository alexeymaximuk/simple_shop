namespace Shop.Users.Exceptions;

public class AccountDeactivatedException(string message) : AppException(message, 403);
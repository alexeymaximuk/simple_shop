namespace Shop.Shared.Exceptions;

public class AccountDeactivatedException(string message) : AppException(message, 403);
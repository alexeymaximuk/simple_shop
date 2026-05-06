namespace Shop.Users.Exceptions;

public class ValidationException(string message) : AppException(message, 422);
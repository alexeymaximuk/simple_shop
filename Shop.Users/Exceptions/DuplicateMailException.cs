namespace Shop.Users.Exceptions;

public class DuplicateMailException(string message) : AppException(message, 409);
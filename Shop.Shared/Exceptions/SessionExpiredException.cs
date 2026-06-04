namespace Shop.Shared.Exceptions;

public class SessionExpiredException(string message) : AppException(message, 401);
namespace Shop.Shared.Exceptions;

public class TokenExpiredException(string message) : AppException(message, 400);
namespace Shop.Shared.Exceptions;

public class AuthorisationException(string message) : AppException(message, 401);
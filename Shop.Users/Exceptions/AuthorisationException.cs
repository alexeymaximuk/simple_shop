namespace Shop.Users.Exceptions;

public class AuthorisationException(string message) : AppException(message, 401);
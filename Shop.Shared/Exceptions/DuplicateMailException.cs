namespace Shop.Shared.Exceptions;

public class DuplicateMailException(string message) : AppException(message, 409);
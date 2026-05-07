namespace Shop.Shared.Exceptions;

public class InvalidRequestException(string message) : AppException(message, 422);
namespace Shop.Shared.Exceptions;

public class ForbiddenException(string message) : AppException(message, 403);
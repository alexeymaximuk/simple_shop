namespace Shop.Users.Exceptions;

public class DuplicateMailException(string message) : Exception(message);
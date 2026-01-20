namespace Infrastructure.Database;

public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

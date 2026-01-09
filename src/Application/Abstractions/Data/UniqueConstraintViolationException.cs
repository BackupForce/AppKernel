namespace Application.Abstractions.Data;

public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

namespace Domain.Exceptions;

public class GreenhouseException : Exception
{
    public GreenhouseException()
    {
    }
    
    public GreenhouseException(string? message) : base(message)
    {
    }

    public GreenhouseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
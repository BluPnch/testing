namespace Domain.Exceptions;

public class ClientNotFoundException : GreenhouseException
{
    public ClientNotFoundException()
    {
    }

    public ClientNotFoundException(string? message) : base(message)
    {
    }
    
    public ClientNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
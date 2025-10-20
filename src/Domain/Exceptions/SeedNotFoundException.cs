namespace Domain.Exceptions;

public class SeedNotFoundException : GreenhouseException
{
    public SeedNotFoundException()
    {
    }

    public SeedNotFoundException(string? message) : base(message)
    {
    }
    
    public SeedNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
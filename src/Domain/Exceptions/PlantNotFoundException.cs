namespace Domain.Exceptions;

public class PlantNotFoundException : GreenhouseException
{
    public PlantNotFoundException()
    {
    }

    public PlantNotFoundException(string? message) : base(message)
    {
    }
    
    public PlantNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
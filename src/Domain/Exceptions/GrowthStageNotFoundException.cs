namespace Domain.Exceptions;

public class GrowthStageNotFoundException : GreenhouseException
{
    public GrowthStageNotFoundException()
    {
    }

    public GrowthStageNotFoundException(string? message) : base(message)
    {
    }
    
    public GrowthStageNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
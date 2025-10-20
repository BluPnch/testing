namespace Domain.Exceptions;

public class AdministratorNotFoundException : GreenhouseException
{
    public AdministratorNotFoundException()
    {
    }

    public AdministratorNotFoundException(string? message) : base(message)
    {
    }
    
    public AdministratorNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
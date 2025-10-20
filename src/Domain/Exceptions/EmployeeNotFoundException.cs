namespace Domain.Exceptions;

public class EmployeeNotFoundException : GreenhouseException
{
    public EmployeeNotFoundException()
    {
    }

    public EmployeeNotFoundException(string? message) : base(message)
    {
    }
    
    public EmployeeNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
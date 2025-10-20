namespace Domain.Exceptions;

public class JournalRecordNotFoundException : GreenhouseException
{
    public JournalRecordNotFoundException()
    {
    }

    public JournalRecordNotFoundException(string? message) : base(message)
    {
    }
    
    public JournalRecordNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
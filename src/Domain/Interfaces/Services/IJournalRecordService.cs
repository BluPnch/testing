using Domain.Models;


namespace Domain.Interfaces.Services;

public interface IJournalRecordService
{
    Task<JournalRecord> CreateJournalRecordAsync(JournalRecord journalRecord);
    
    Task<IEnumerable<JournalRecord>> GetAllJournalRecordsAsync();
    
    Task<JournalRecord?> GetJournalRecordByIdAsync(Guid id);
    
    Task UpdateJournalRecordAsync(JournalRecord journalRecord);
    
    Task DeleteJournalRecordAsync(Guid id);

    Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId);
    
    Task<IEnumerable<JournalRecord>> GetJournalRecordsByDateRangeAsync(DateTime? startDate, DateTime? endDate);
}
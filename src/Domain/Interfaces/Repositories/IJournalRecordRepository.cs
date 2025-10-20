using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IJournalRecordRepository
{
    Task<IEnumerable<JournalRecord>> GetAllJournalRecordsAsync();

    Task<JournalRecord?> GetJournalRecordByIdAsync(Guid id);

    Task<JournalRecord> AddJournalRecordAsync(JournalRecord journalRecord);

    Task UpdateJournalRecordAsync(JournalRecord journalRecord);

    Task DeleteJournalRecordAsync(Guid id);

    Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId);
    
    Task<IEnumerable<JournalRecord>> GetJournalRecordsByDateRangeAsync(DateTime? startDate, DateTime? endDate);
}
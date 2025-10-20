using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace DataAccess.Repositories
{
    public class JournalRecordRepository : IJournalRecordRepository
    {
        private readonly GreenhouseContext _context;

        public JournalRecordRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<JournalRecord>> GetAllJournalRecordsAsync()
        {
            var records = await _context.JournalRecords
                .AsNoTracking()
                .Include(j => j.Plant)
                .Include(j => j.GrowthStage)
                .ToListAsync();

            return records.ToDomain();
        }

        public async Task<JournalRecord?> GetJournalRecordByIdAsync(Guid id)
        {
            var record = await _context.JournalRecords
                .AsNoTracking()
                .Include(j => j.Plant)
                .Include(j => j.GrowthStage)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (record == null)
                throw new JournalRecordNotFoundException($"Journal record with id '{id}' not found");

            return record.ToDomain();
        }

        public async Task<JournalRecord> AddJournalRecordAsync(JournalRecord journalRecord)
        {
            if (journalRecord == null)
                throw new ArgumentNullException(nameof(journalRecord));

            var journalRecordDb = journalRecord.ToDb();
            
            await _context.JournalRecords.AddAsync(journalRecordDb!);
            await _context.SaveChangesAsync();

            return journalRecordDb.ToDomain()!;
        }

        public async Task UpdateJournalRecordAsync(JournalRecord journalRecord)
        {
            if (journalRecord == null)
                throw new ArgumentNullException(nameof(journalRecord));

            var existingRecord = await _context.JournalRecords
                .FirstOrDefaultAsync(j => j.Id == journalRecord.Id);

            if (existingRecord == null)
                throw new JournalRecordNotFoundException($"Journal record with id '{journalRecord.Id}' not found");

            existingRecord.PlantHeight = journalRecord.PlantHeight;
            existingRecord.FruitCount = journalRecord.FruitCount;
            existingRecord.Condition = journalRecord.Condition;
            existingRecord.Date = journalRecord.Date;

            _context.JournalRecords.Update(existingRecord);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteJournalRecordAsync(Guid id)
        {
            var record = await _context.JournalRecords.FindAsync(id);
            
            if (record == null)
                throw new JournalRecordNotFoundException($"Journal record with id '{id}' not found");

            _context.JournalRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId)
        {
            var records = await _context.JournalRecords
                .AsNoTracking()
                .Include(j => j.Plant)
                .Include(j => j.GrowthStage)
                .Where(j => j.PlantId == plantId)
                .OrderByDescending(j => j.Date)
                .ToListAsync();

            return records.ToDomain();
        }
        
        public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.JournalRecords
                .AsNoTracking()
                .Include(j => j.Plant)
                .Include(j => j.GrowthStage)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(j => j.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(j => j.Date <= endDate.Value);
            }

            var records = await query
                .OrderByDescending(j => j.Date)
                .ToListAsync();

            return records.ToDomain();
        }


        // public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByEmployeeIdAsync(Guid employeeId)
        // {
        //     var records = await _context.JournalRecords
        //         .AsNoTracking()
        //         .Include(j => j.Plant)
        //         .Include(j => j.GrowthStage)
        //         .Where(j => j.EmployeeId == employeeId)
        //         .OrderByDescending(j => j.Date)
        //         .ToListAsync();
        //
        //     return records.ToDomain();
        // }
    }
}
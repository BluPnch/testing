using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class JournalRecordService : IJournalRecordService
{
    private readonly IJournalRecordRepository _journalRecordRepository;
    private readonly IPlantRepository _plantRepository;
    private readonly JournalRecordValidator _journalRecordValidator;
    private readonly ILogger<JournalRecordService> _logger;

    public JournalRecordService(
        IJournalRecordRepository journalRecordRepository,
        IPlantRepository plantRepository,
        JournalRecordValidator journalRecordValidator,
        ILogger<JournalRecordService> logger,
        IConfiguration configuration)
    {
        _journalRecordRepository = journalRecordRepository ?? throw new ArgumentNullException(nameof(journalRecordRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _journalRecordValidator = journalRecordValidator ?? throw new ArgumentNullException(nameof(journalRecordValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JournalRecord> CreateJournalRecordAsync(JournalRecord record)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        _logger.LogInformation("Попытка создания записи журнала с ID: {RecordId}", record.Id);
        try
        {
            var plant = await _plantRepository.GetPlantByIdAsync(record.PlantId);
            if (plant == null)
            {
                throw new ArgumentException("Растение не найдено", nameof(record.PlantId));
            }

            await _journalRecordValidator.ValidateAndThrowAsync(record);
            var result = await _journalRecordRepository.AddJournalRecordAsync(record);
            _logger.LogInformation("Запись журнала успешно создана с ID: {RecordId}", record.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании записи журнала с ID: {RecordId}", record.Id);
            throw;
        }
    }

    public async Task<IEnumerable<JournalRecord>> GetAllJournalRecordsAsync()
    {
        _logger.LogInformation("Получение списка всех записей журнала");
        try
        {
            var records = await _journalRecordRepository.GetAllJournalRecordsAsync();
            _logger.LogInformation("Успешно получено {Count} записей журнала", records.Count());
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка записей журнала");
            throw;
        }
    }

    public async Task<JournalRecord?> GetJournalRecordByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение записи журнала по ID: {RecordId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения записи журнала с пустым ID");
            throw new ArgumentException("ID записи журнала не может быть пустым", nameof(id));
        }

        try
        {
            var record = await _journalRecordRepository.GetJournalRecordByIdAsync(id);
            _logger.LogInformation("Успешно получена запись журнала с ID: {RecordId}", id);
            return record;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записи журнала с ID: {RecordId}", id);
            throw;
        }
    }

    public async Task UpdateJournalRecordAsync(JournalRecord record)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        _logger.LogInformation("Попытка обновления записи журнала с ID: {RecordId}", record.Id);
        try
        {
            await _journalRecordValidator.ValidateAndThrowAsync(record);
            await _journalRecordRepository.UpdateJournalRecordAsync(record);
            _logger.LogInformation("Запись журнала успешно обновлена с ID: {RecordId}", record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении записи журнала с ID: {RecordId}", record.Id);
            throw;
        }
    }

    public async Task DeleteJournalRecordAsync(Guid id)
    {
        _logger.LogInformation("Попытка удаления записи журнала с ID: {RecordId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка удаления записи журнала с пустым ID");
            throw new ArgumentException("ID записи журнала не может быть пустым", nameof(id));
        }

        try
        {
            var record = await _journalRecordRepository.GetJournalRecordByIdAsync(id);
            if (record == null)
            {
                _logger.LogWarning("Запись журнала с ID {RecordId} не найдена", id);
                throw new KeyNotFoundException($"Запись журнала с ID {id} не найдена");
            }

            await _journalRecordRepository.DeleteJournalRecordAsync(id);
            _logger.LogInformation("Запись журнала с ID {RecordId} успешно удалена", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении записи журнала с ID: {RecordId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId)
    {
        _logger.LogInformation("Получение записей журнала для растения с ID: {PlantId}", plantId);
        
        if (plantId == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения записей журнала для растения с пустым ID");
            throw new ArgumentException("ID растения не может быть пустым", nameof(plantId));
        }

        try
        {
            var records = await _journalRecordRepository.GetJournalRecordsByPlantIdAsync(plantId);
            _logger.LogInformation("Успешно получено {Count} записей журнала для растения с ID: {PlantId}", 
                records.Count(), plantId);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей журнала для растения с ID: {PlantId}", plantId);
            throw;
        }
    }
    
    public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        _logger.LogInformation("Получение записей журнала за период: {StartDate} - {EndDate}", startDate, endDate);
    
        try
        {
            var records = await _journalRecordRepository.GetJournalRecordsByDateRangeAsync(startDate, endDate);
            _logger.LogInformation("Успешно получено {Count} записей журнала за указанный период", records.Count());
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей журнала за период: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }
}
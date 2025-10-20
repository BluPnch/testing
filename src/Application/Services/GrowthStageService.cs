using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class GrowthStageService : IGrowthStageService
{
    private readonly IGrowthStageRepository _growthStageRepository;
    private readonly GrowthStageValidator _growthStageValidator;
    private readonly ILogger<GrowthStageService> _logger;

    public GrowthStageService(
        IGrowthStageRepository growthStageRepository,
        GrowthStageValidator growthStageValidator,
        ILogger<GrowthStageService> logger,
        IConfiguration configuration)
    {
        _growthStageRepository = growthStageRepository ?? throw new ArgumentNullException(nameof(growthStageRepository));
        _growthStageValidator = growthStageValidator ?? throw new ArgumentNullException(nameof(growthStageValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GrowthStage> CreateGrowthStageAsync(GrowthStage growthStage)
    {
        if (growthStage == null)
        {
            throw new ArgumentNullException(nameof(growthStage));
        }

        _logger.LogInformation("Попытка создания стадии роста с ID: {StageId}", growthStage.Id);
        try
        {
            await _growthStageValidator.ValidateAndThrowAsync(growthStage);
            var result = await _growthStageRepository.CreateGrowthStageAsync(growthStage);
            _logger.LogInformation("Стадия роста успешно создана с ID: {StageId}", growthStage.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании стадии роста с ID: {StageId}", growthStage.Id);
            throw;
        }
    }

    public async Task<IEnumerable<GrowthStage>> GetAllGrowthStagesAsync()
    {
        _logger.LogInformation("Retrieving all growth stages");
        try
        {
            var stages = await _growthStageRepository.GetAllGrowthStagesAsync();
            _logger.LogInformation("Successfully retrieved {Count} growth stages", stages.Count());
            return stages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all growth stages");
            throw new ApplicationException("Failed to retrieve growth stages", ex);
        }
    }

    public async Task<GrowthStage> GetGrowthStageByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение стадии роста по ID: {StageId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения стадии роста с пустым ID");
            throw new ArgumentException("ID стадии роста не может быть пустым", nameof(id));
        }

        try
        {
            var stage = await _growthStageRepository.GetGrowthStageByIdAsync(id);
            _logger.LogInformation("Успешно получена стадия роста с ID: {StageId}", id);
            return stage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении стадии роста с ID: {StageId}", id);
            throw;
        }
    }
    
    public async Task<GrowthStage> GetGrowthStageByNameAsync(string name)
    {
        _logger.LogInformation("Получение стадии роста по названию: {StageName}", name);
    
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Попытка получения стадии роста с пустым названием");
            throw new ArgumentException("Название стадии роста не может быть пустым", nameof(name));
        }

        try
        {
            var stage = await _growthStageRepository.GetGrowthStageByNameAsync(name);
        
            if (stage == null)
            {
                _logger.LogWarning("Стадия роста с названием '{StageName}' не найдена", name);
                throw new KeyNotFoundException($"Стадия роста с названием '{name}' не найдена");
            }
        
            _logger.LogInformation("Успешно получена стадия роста с названием: {StageName}", name);
            return stage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении стадии роста с названием: {StageName}", name);
            throw;
        }
    }
}
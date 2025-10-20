using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class SeedService : ISeedService
{
    private readonly ISeedRepository _seedRepository;
    private readonly IPlantRepository _plantRepository;
    private readonly SeedValidator _seedValidator;
    private readonly ILogger<SeedService> _logger;

    public SeedService(
        ISeedRepository seedRepository,
        IPlantRepository plantRepository,
        SeedValidator seedValidator,
        ILogger<SeedService> logger,
        IConfiguration configuration)
    {
        _seedRepository = seedRepository ?? throw new ArgumentNullException(nameof(seedRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _seedValidator = seedValidator ?? throw new ArgumentNullException(nameof(seedValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Seed> CreateSeedAsync(Seed seed)
    {
        if (seed == null)
        {
            _logger.LogWarning("Attempted to create null seed");
            throw new ArgumentNullException(nameof(seed));
        }

        _logger.LogInformation("Attempting to create seed with ID: {SeedId}", seed.Id);
        try
        {
            await _seedValidator.ValidateAndThrowAsync(seed);
            var plant = await _plantRepository.GetPlantByIdAsync(seed.PlantId);
            if (plant == null)
            {
                _logger.LogWarning("Plant with ID {PlantId} not found", seed.PlantId);
                throw new KeyNotFoundException($"Растение с ID {seed.PlantId} не найдено");
            }

            var result = await _seedRepository.CreateSeedAsync(seed);
            _logger.LogInformation("Successfully created seed with ID: {SeedId}", seed.Id);
            return result;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seed with ID: {SeedId}", seed.Id);
            throw new ApplicationException("Failed to create seed", ex);
        }
    }

    public async Task<Seed> GetSeedByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение семени по ID: {SeedId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения семени с пустым ID");
            throw new ArgumentException("ID семени не может быть пустым", nameof(id));
        }

        try
        {
            var seed = await _seedRepository.GetSeedByIdAsync(id);
            _logger.LogInformation("Успешно получено семя с ID: {SeedId}", id);
            return seed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении семени с ID: {SeedId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Seed>> GetAllSeedsAsync()
    {
        _logger.LogInformation("Retrieving all seeds");
        try
        {
            var seeds = await _seedRepository.GetAllSeedsAsync();
            _logger.LogInformation("Successfully retrieved {Count} seeds", seeds.Count());
            return seeds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all seeds");
            throw new ApplicationException("Failed to retrieve seeds", ex);
        }
    }

    public async Task DeleteSeedAsync(Guid id)
    {
        _logger.LogInformation("Попытка удаления семени с ID: {SeedId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка удаления семени с пустым ID");
            throw new ArgumentException("ID семени не может быть пустым", nameof(id));
        }

        try
        {
            var seed = await _seedRepository.GetSeedByIdAsync(id);
            if (seed == null)
            {
                _logger.LogWarning("Семя с ID {SeedId} не найдено", id);
                throw new KeyNotFoundException($"Семя с ID {id} не найдено");
            }

            await _seedRepository.DeleteSeedAsync(id);
            _logger.LogInformation("Семя с ID {SeedId} успешно удалено", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении семени с ID: {SeedId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Seed>> GetSeedsByMaturityAsync(string maturity)
    {
        _logger.LogInformation("Retrieving seeds with maturity: {Maturity}", maturity);
        
        if (string.IsNullOrWhiteSpace(maturity))
        {
            _logger.LogWarning("Attempted to retrieve seeds with empty maturity");
            throw new ArgumentException("Maturity cannot be empty", nameof(maturity));
        }

        try
        {
            var seeds = await _seedRepository.GetSeedsByMaturityAsync(maturity);
            _logger.LogInformation("Successfully retrieved {Count} seeds with maturity: {Maturity}", 
                seeds.Count(), maturity);
            return seeds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seeds with maturity: {Maturity}", maturity);
            throw;
        }
    }

    public async Task<IEnumerable<Seed>> GetSeedsByViabilityAsync(string viability)
    {
        _logger.LogInformation("Retrieving seeds with viability: {Viability}", viability);
        
        if (string.IsNullOrWhiteSpace(viability))
        {
            _logger.LogWarning("Attempted to retrieve seeds with empty viability");
            throw new ArgumentException("Viability cannot be empty", nameof(viability));
        }

        try
        {
            var seeds = await _seedRepository.GetSeedsByViabilityAsync(viability);
            _logger.LogInformation("Successfully retrieved {Count} seeds with viability: {Viability}", 
                seeds.Count(), viability);
            return seeds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seeds with viability: {Viability}", viability);
            throw;
        }
    }

    public async Task<Plant> GetPlantBySeedIdAsync(Guid seedId)
    {
        _logger.LogInformation("Получение растения по ID семени: {SeedId}", seedId);
        
        if (seedId == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения растения по пустому ID семени");
            throw new ArgumentException("ID семени не может быть пустым", nameof(seedId));
        }

        try
        {
            var plant = await _seedRepository.GetPlantBySeedIdAsync(seedId);
            _logger.LogInformation("Успешно получено растение для семени с ID: {SeedId}", seedId);
            return plant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении растения для семени с ID: {SeedId}", seedId);
            throw;
        }
    }

    public async Task<Seed> UpdateSeedAsync(Seed seed)
    {
        _logger.LogInformation("Попытка обновления семени с ID: {SeedId}", seed.Id);
        try
        {
            if (seed == null)
            {
                _logger.LogWarning("Попытка обновления null-семени");
                throw new ArgumentNullException(nameof(seed));
            }

            await _seedValidator.ValidateAndThrowAsync(seed);

            var existingSeed = await _seedRepository.GetSeedByIdAsync(seed.Id);
            if (existingSeed == null)
            {
                _logger.LogWarning("Семя с ID {SeedId} не найдено", seed.Id);
                throw new KeyNotFoundException($"Семя с ID {seed.Id} не найдено");
            }

            var plant = await _plantRepository.GetPlantByIdAsync(seed.PlantId);
            if (plant == null)
            {
                _logger.LogWarning("Растение с ID {PlantId} не найдено", seed.PlantId);
                throw new KeyNotFoundException($"Растение с ID {seed.PlantId} не найдено");
            }

            var result = await _seedRepository.UpdateSeedAsync(seed);
            _logger.LogInformation("Семя с ID {SeedId} успешно обновлено", seed.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении семени с ID: {SeedId}", seed.Id);
            throw;
        }
    }
}
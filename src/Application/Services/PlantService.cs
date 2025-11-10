using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;
    private readonly IClientRepository _clientRepository;
    private readonly PlantValidator _plantValidator;
    private readonly ILogger<PlantService> _logger;

    public PlantService(
        IPlantRepository plantRepository,
        IClientRepository clientRepository,
        PlantValidator plantValidator,
        ILogger<PlantService> logger,
        IConfiguration configuration)
    {
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _plantValidator = plantValidator ?? throw new ArgumentNullException(nameof(plantValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Plant> CreatePlantAsync(Plant plant)
    {
        if (plant == null)
        {
            _logger.LogWarning("Attempted to create null plant");
            throw new ArgumentNullException(nameof(plant));
        }

        if (plant.ClientId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to create plant with empty client ID");
            throw new ArgumentException("Client ID cannot be empty", nameof(plant.ClientId));
        }

        _logger.LogInformation("Attempting to create plant with ID: {PlantId} for client: {ClientId}", plant.Id, plant.ClientId);
        try
        {
            var client = await _clientRepository.GetClientByIdAsync(plant.ClientId);
            if (client == null)
            {
                _logger.LogWarning("Client with ID {ClientId} not found", plant.ClientId);
                throw new ArgumentException("Клиент не найден", nameof(plant.ClientId));
            }

            await _plantValidator.ValidateAndThrowAsync(plant);
            var result = await _plantRepository.CreatePlantAsync(plant);
            _logger.LogInformation("Successfully created plant with ID: {PlantId} for client: {ClientId}", plant.Id, plant.ClientId);
            return result;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error creating plant with ID: {PlantId} for client: {ClientId}", plant.Id, plant.ClientId);
            throw new ApplicationException("Failed to create plant", ex);
        }
    }

    public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
    {
        _logger.LogInformation("Retrieving all plants");
        try
        {
            var plants = await _plantRepository.GetAllPlantsAsync();
            _logger.LogInformation("Successfully retrieved {Count} plants", plants.Count());
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all plants");
            throw new ApplicationException("Failed to retrieve plants", ex);
        }
    }

    public async Task<Plant> GetPlantByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving plant with ID: {PlantId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(id));
        }

        try
        {
            var plant = await _plantRepository.GetPlantByIdAsync(id);
            _logger.LogInformation("Successfully retrieved plant with ID: {PlantId}", id);
            return plant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plant with ID: {PlantId}", id);
            throw;
        }
    }

    public async Task<Plant> UpdatePlantAsync(Plant plant)
    {
        if (plant == null)
        {
            _logger.LogWarning("Attempted to update null plant");
            throw new ArgumentNullException(nameof(plant));
        }

        _logger.LogInformation("Attempting to update plant with ID: {PlantId}", plant.Id);
        
        if (plant.Id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to update plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(plant.Id));
        }

        try
        {
            await _plantValidator.ValidateAndThrowAsync(plant);
            var result = await _plantRepository.UpdatePlantAsync(plant);
            _logger.LogInformation("Successfully updated plant with ID: {PlantId}", plant.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plant with ID: {PlantId}", plant.Id);
            throw;
        }
    }

    public async Task DeletePlantAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete plant with ID: {PlantId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to delete plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(id));
        }

        try
        {
            var plant = await _plantRepository.GetPlantByIdAsync(id);
            if (plant == null)
            {
                _logger.LogWarning("Plant with ID {PlantId} not found", id);
                throw new KeyNotFoundException($"Plant with ID {id} not found");
            }

            await _plantRepository.DeletePlantAsync(id);
            _logger.LogInformation("Successfully deleted plant with ID: {PlantId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plant with ID: {PlantId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Plant>> GetPlantsByFamilyAsync(string family)
    {
        _logger.LogInformation("Retrieving plants with family: {Family}", family);
        
        if (string.IsNullOrWhiteSpace(family))
        {
            _logger.LogWarning("Attempted to retrieve plants with empty family");
            throw new ArgumentException("Family cannot be empty", nameof(family));
        }

        try
        {
            var plants = await _plantRepository.GetPlantsByFamilyAsync(family);
            _logger.LogInformation("Successfully retrieved {Count} plants with family: {Family}", 
                plants.Count(), family);
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plants with family: {Family}", family);
            throw;
        }
    }

    public async Task<IEnumerable<Plant>> GetPlantsBySpeciesAsync(string species)
    {
        _logger.LogInformation("Retrieving plants with species: {Species}", species);
        
        if (string.IsNullOrWhiteSpace(species))
        {
            _logger.LogWarning("Attempted to retrieve plants with empty species");
            throw new ArgumentException("Species cannot be empty", nameof(species));
        }

        try
        {
            var plants = await _plantRepository.GetPlantsBySpeciesAsync(species);
            _logger.LogInformation("Successfully retrieved {Count} plants with species: {Species}", 
                plants.Count(), species);
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plants with species: {Species}", species);
            throw;
        }
    }

    public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId)
    {
        _logger.LogInformation("Retrieving journal records for plant with ID: {PlantId}", plantId);
        
        if (plantId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve journal records for plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(plantId));
        }

        try
        {
            var records = await _plantRepository.GetJournalRecordsByPlantIdAsync(plantId);
            _logger.LogInformation("Successfully retrieved {Count} journal records for plant with ID: {PlantId}", 
                records.Count(), plantId);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal records for plant with ID: {PlantId}", plantId);
            throw;
        }
    }

    public async Task<IEnumerable<Seed>> GetSeedsByPlantIdAsync(Guid plantId)
    {
        _logger.LogInformation("Retrieving seeds for plant with ID: {PlantId}", plantId);
        
        if (plantId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve seeds for plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(plantId));
        }

        try
        {
            var seeds = await _plantRepository.GetSeedsByPlantIdAsync(plantId);
            _logger.LogInformation("Successfully retrieved {Count} seeds for plant with ID: {PlantId}", 
                seeds.Count(), plantId);
            return seeds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seeds for plant with ID: {PlantId}", plantId);
            throw;
        }
    }
}
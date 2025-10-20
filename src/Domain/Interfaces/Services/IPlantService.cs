using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IPlantService
{
    Task<Plant> CreatePlantAsync(Plant plant, Guid clientId);

    Task<Plant> GetPlantByIdAsync(Guid id);

    Task<IEnumerable<Plant>> GetAllPlantsAsync();

    Task<Plant> UpdatePlantAsync(Plant plant);

    Task DeletePlantAsync(Guid id);

    Task<IEnumerable<Plant>> GetPlantsByFamilyAsync(string family);
    
    Task<IEnumerable<Plant>> GetPlantsBySpeciesAsync(string species);
    
    Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId);

    Task<IEnumerable<Seed>> GetSeedsByPlantIdAsync(Guid plantId);
}
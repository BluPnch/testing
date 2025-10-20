using Domain.Models;


namespace Domain.Interfaces.Services;

public interface ISeedService
{
    Task<Seed> CreateSeedAsync(Seed seed);

    Task<Seed> GetSeedByIdAsync(Guid id);

    Task<IEnumerable<Seed>> GetAllSeedsAsync();

    Task DeleteSeedAsync(Guid id);

    Task<IEnumerable<Seed>> GetSeedsByMaturityAsync(string maturity);

    Task<IEnumerable<Seed>> GetSeedsByViabilityAsync(string viability);
    
    Task<Plant> GetPlantBySeedIdAsync(Guid seedId);

    Task<Seed> UpdateSeedAsync(Seed seed);
}
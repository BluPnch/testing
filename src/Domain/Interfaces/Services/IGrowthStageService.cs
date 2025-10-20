using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IGrowthStageService
{
    Task<IEnumerable<GrowthStage>> GetAllGrowthStagesAsync();
    
    Task<GrowthStage> GetGrowthStageByIdAsync(Guid id);
    
    Task<GrowthStage> GetGrowthStageByNameAsync(string name);
}
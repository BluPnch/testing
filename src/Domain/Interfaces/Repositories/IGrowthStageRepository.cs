using Domain.Models;


namespace Domain.Interfaces.Repositories;

public interface IGrowthStageRepository
{
    /// <summary>
    /// Споздать новый этап роста.
    /// </summary>
    /// <returns>Новый этап роста.</returns>
    Task<GrowthStage> CreateGrowthStageAsync(GrowthStage growthStage);
    
    /// <summary>
    /// Получить все этапы роста.
    /// </summary>
    /// <returns>Список всех этапов роста.</returns>
    Task<IEnumerable<GrowthStage>> GetAllGrowthStagesAsync();
    
    /// <summary>
    /// Получить этап роста по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор этапа роста.</param>
    /// <returns>Этап роста с указанным идентификатором.</returns>
    Task<GrowthStage> GetGrowthStageByIdAsync(Guid id);
    
    /// <summary>
    /// Получить этап роста по его названию.
    /// </summary>
    /// <param name="name">Название этапа роста.</param>
    /// <returns>Этап роста с указанным названием.</returns>
    Task<GrowthStage> GetGrowthStageByNameAsync(string name);

}
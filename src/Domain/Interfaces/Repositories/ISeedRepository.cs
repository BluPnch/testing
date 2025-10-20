using Domain.Models;


namespace Domain.Interfaces.Repositories;

public interface ISeedRepository
{
    /// <summary>
    /// Создать новое семечко.
    /// </summary>
    /// <param name="seed">Данные семечка.</param>
    /// <returns>Созданное семечко.</returns>
    Task<Seed> CreateSeedAsync(Seed seed);

    /// <summary>
    /// Получить семечко по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор семечка.</param>
    /// <returns>Семечко с указанным идентификатором.</returns>
    Task<Seed> GetSeedByIdAsync(Guid id);

    /// <summary>
    /// Получить все семечки.
    /// </summary>
    /// <returns>Список всех семечек.</returns>
    Task<IEnumerable<Seed>> GetAllSeedsAsync();

    /// <summary>
    /// Удалить семечки по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор семян.</param>
    /// <returns></returns>
    Task DeleteSeedAsync(Guid id);

    /// <summary>
    /// Получить семечки по их зрелости.
    /// </summary>
    /// <param name="maturity">Зрелость семян.</param>
    /// <returns>Список семян с указанной зрелостью.</returns>
    Task<IEnumerable<Seed>> GetSeedsByMaturityAsync(string maturity);

    /// <summary>
    /// Получить семечки по их жизнеспособности.
    /// </summary>
    /// <param name="viability">Жизнеспособность семян.</param>
    /// <returns>Список семян с указанной жизнеспособностью.</returns>
    Task<IEnumerable<Seed>> GetSeedsByViabilityAsync(string viability);
    
    /// <summary>
    /// Получить растение, от которого получены семена.
    /// </summary>
    /// <param name="seedId">Идентификатор семян.</param>
    /// <returns>Растение.</returns>
    Task<Plant> GetPlantBySeedIdAsync(Guid seedId);

    /// <summary>
    /// Обновить данные семени.
    /// </summary>
    /// <param name="seed">Обновленные данные семени.</param>
    /// <returns>Обновленное семя.</returns>
    Task<Seed> UpdateSeedAsync(Seed seed);
}
using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IPlantRepository
{
    /// <summary>
    /// Создать новое растение.
    /// </summary>
    /// <param name="plant">Данные растения.</param>
    /// <param name="clientId">ID клиента-владельца растения.</param>
    /// <returns>Созданное растение.</returns>
    Task<Plant> CreatePlantAsync(Plant plant, Guid clientId);

    /// <summary>
    /// Получить растение по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <returns>Растение с указанным идентификатором.</returns>
    Task<Plant> GetPlantByIdAsync(Guid id);

    /// <summary>
    /// Получить все растения.
    /// </summary>
    /// <returns>Список всех растений.</returns>
    Task<IEnumerable<Plant>> GetAllPlantsAsync();

    /// <summary>
    /// Обновить данные растения.
    /// </summary>
    /// <param name="plant">Обновленные данные растения.</param>
    /// <returns>Обновленное растение.</returns>
    Task<Plant> UpdatePlantAsync(Plant plant);

    /// <summary>
    /// Удалить растение по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <returns></returns>
    Task DeletePlantAsync(Guid id);

    /// <summary>
    /// Получить растения по их семейству.
    /// </summary>
    /// <param name="family">Семейство растений.</param>
    /// <returns>Список растений, принадлежащих указанному семейству.</returns>
    // Для обозначения сферы растений для сотрудников
    Task<IEnumerable<Plant>> GetPlantsByFamilyAsync(string family);
    

    /// <summary>
    /// Получить растения по их виду.
    /// </summary>
    /// <param name="species">Вид растений.</param>
    /// <returns>Список растений, принадлежащих указанному виду.</returns>
    Task<IEnumerable<Plant>> GetPlantsBySpeciesAsync(string species);
    
    /// <summary>
    /// Получить записи журнала для растения.
    /// </summary>
    /// <param name="plantId">Идентификатор растения.</param>
    /// <returns>Список записей журнала.</returns>
    Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId);

    /// <summary>
    /// Получить семена, полученные от растения.
    /// </summary>
    /// <param name="plantId">Идентификатор растения.</param>
    /// <returns>Список семян.</returns>
    Task<IEnumerable<Seed>> GetSeedsByPlantIdAsync(Guid plantId);

    /// <summary>
    /// Получить растения, принадлежащие клиенту.
    /// </summary>
    /// <param name="clientId">Идентификатор клиента.</param>
    /// <returns>Список растений клиента.</returns>
    Task<IEnumerable<Plant>> GetPlantsByClientIdAsync(Guid clientId);
}
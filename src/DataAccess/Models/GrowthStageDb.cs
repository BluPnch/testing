namespace DataAccess.Models;


public class GrowthStageDb
{
    public Guid Id { get; protected set; }
    
    public string Name { get; set; }

    public string Description { get; set; }
    
    
    public virtual ICollection<JournalRecordDb> JournalRecords { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра этапа роста.
    /// </summary>
    /// <param name="id">Идентификатор этапа роста.</param>
    /// <param name="name">Название этапа роста.</param>
    /// <param name="description">Описание этапа роста.</param>
    public GrowthStageDb(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
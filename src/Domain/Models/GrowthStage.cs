namespace Domain.Models;


public class GrowthStage : BaseModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра этапа роста.
    /// </summary>
    /// <param name="id">Идентификатор этапа роста.</param>
    /// <param name="name">Название этапа роста.</param>
    /// <param name="description">Описание этапа роста.</param>
    public GrowthStage(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }
}
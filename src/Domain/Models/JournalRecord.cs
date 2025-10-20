namespace Domain.Models;


public class JournalRecord : BaseModel
{
    public Guid PlantId { get; set; }

    public Guid GrowthStageId { get; set; }

    public Guid EmployeeId { get; set; }

    public double PlantHeight { get; set; }

    public int FruitCount { get; set; }

    public EnumCondition Condition { get; set; }
    
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра записи в журнале.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <param name="plantId">Идентификатор растения.</param>
    /// <param name="growthStageId">Идентификатор этапа роста.</param>
    /// <param name="employeeId">Идентификатор сотрудника.</param>
    /// <param name="plantHeight">Высота растения.</param>
    /// <param name="fruitCount">Количество плодов.</param>
    /// <param name="condition">Состояние растения.</param>
    /// <param name="date">Время создания записи.</param>
    public JournalRecord(Guid id, Guid plantId, Guid growthStageId, Guid employeeId, double plantHeight, int fruitCount, EnumCondition condition, DateTimeOffset date) : base(id)
    {
        PlantId = plantId;
        GrowthStageId = growthStageId;
        EmployeeId = employeeId;
        PlantHeight = plantHeight;
        FruitCount = fruitCount;
        Condition = condition;
        Date = date;
    }
}
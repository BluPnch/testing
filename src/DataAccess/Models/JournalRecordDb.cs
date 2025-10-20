namespace DataAccess.Models;

public class JournalRecordDb
{
    public Guid Id { get; protected set; }
    
    public Guid PlantId { get; set; }
    
    public Guid GrowthStageId { get; protected set; }
    
    public Guid EmployeeId { get; protected set; }

    public double PlantHeight { get; set; }

    public int FruitCount { get; set; }

    public EnumCondition Condition { get; set; }
    
    public DateTimeOffset Date { get; set; }
    
    
    
    public virtual PlantDb Plant { get; protected set; }

    public virtual GrowthStageDb GrowthStage { get; protected set; }
    
    public virtual EmployeeDb Employee { get; protected set; }
    
    
    /// <summary>
    /// Конструктор для создания экземпляра записи в журнале.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <param name="plantId"></param>
    /// <param name="plantHeight">Высота растения.</param>
    /// <param name="fruitCount">Количество плодов.</param>
    /// <param name="condition">Состояние растения.</param>
    /// <param name="date">Время создания записи.</param>
    public JournalRecordDb(Guid id, double plantHeight, int fruitCount, EnumCondition condition, DateTimeOffset date, Guid plantId, Guid growthStageId, Guid employeeId)
    {
        Id = id;
        PlantHeight = plantHeight;
        FruitCount = fruitCount;
        Condition = condition;
        Date = date;
        PlantId = plantId;
        GrowthStageId = growthStageId;
        EmployeeId = employeeId;
    }
}
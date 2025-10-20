using DefaultNamespace;


namespace DataAccess.Models;


public class PlantDb
{
    public Guid Id { get; protected set; }
    
    public Guid ClientId { get; protected set; }
    
    public string Specie { get; set; }

    public string Family { get; set; }

    public EnumFlowers Flower { get; set; }

    public EnumFruit Fruit { get; set; }

    public EnumReproduction Reproduction { get; set; }
    
    
    
    public virtual ClientDb Client { get; set; }

    public virtual ICollection<EmployeePlantDb> EmployeePlants { get; set; }

    public virtual ICollection<JournalRecordDb> JournalRecords { get; set; }

    public virtual ICollection<SeedDb> Seeds { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра растения.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <param name="clientId"></param>
    /// <param name="specie">Вид растения.</param>
    /// <param name="family">Семейство растения.</param>
    /// <param name="flower">Цветок растения.</param>
    /// <param name="fruit">Плод растения.</param>
    /// <param name="reproduction">Способ размножения растения.</param>
    public PlantDb(Guid id, Guid clientId, string specie, string family, EnumFlowers flower, EnumFruit fruit, EnumReproduction reproduction)
    {
        Id = id;
        ClientId = clientId;
        Specie = specie;
        Family = family;
        Flower = flower;
        Fruit = fruit;
        Reproduction = reproduction;
    }
}
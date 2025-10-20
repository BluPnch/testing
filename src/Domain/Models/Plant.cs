using DefaultNamespace;


namespace Domain.Models;


public class Plant : BaseModel
{
    public Guid ClientId { get; set; }
    
    public string Specie { get; set; }

    public string Family { get; set; }

    public EnumFlowers Flower { get; set; }

    public EnumFruit Fruit { get; set; }

    public EnumReproduction Reproduction { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра растения.
    /// </summary>
    /// <param name="id">Идентификатор растения.</param>
    /// <param name="specie">Вид растения.</param>
    /// <param name="family">Семейство растения.</param>
    /// <param name="flower">Цветок растения.</param>
    /// <param name="fruit">Плод растения.</param>
    /// <param name="reproduction">Способ размножения растения.</param>
    public Plant(Guid id, Guid clientId, string specie, string family, EnumFlowers flower, EnumFruit fruit, EnumReproduction reproduction) : base(id)
    {
        ClientId = clientId;
        Specie = specie;
        Family = family;
        Flower = flower;
        Fruit = fruit;
        Reproduction = reproduction;
    }
}
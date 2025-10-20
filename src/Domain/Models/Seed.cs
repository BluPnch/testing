using Domain.Models.Enums;


namespace Domain.Models;


public class Seed : BaseModel
{
    public Guid PlantId { get; set; }

    public string Maturity { get; set; }

    public EnumViability Viability { get; set; }

    public EnumLight LightRequirements { get; set; }

    public string WaterRequirements { get; set; }

    public int TemperatureRequirements { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра семян.
    /// </summary>
    /// <param name="id">Идентификатор семян.</param>
    /// <param name="plantId"></param>
    /// <param name="maturity">Зрелость семян.</param>
    /// <param name="viability">Жизнеспособность семян.</param>
    /// <param name="lightRequirements">Требования к свету.</param>
    /// <param name="waterRequirements">Требования к воде.</param>
    /// <param name="temperatureRequirements">Требования к температуре.</param>
    public Seed(Guid id, Guid plantId, string maturity, EnumViability viability, EnumLight lightRequirements, string waterRequirements, int temperatureRequirements) : base(id)
    {
        PlantId = plantId;
        Maturity = maturity;
        Viability = viability;
        LightRequirements = lightRequirements;
        WaterRequirements = waterRequirements;
        TemperatureRequirements = temperatureRequirements;
    }
}
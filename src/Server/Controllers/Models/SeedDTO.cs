namespace Server.Controllers.Models;

public class SeedDTO : BaseModelDTO
{
    public Guid PlantId { get; set; }
    public string Maturity { get; set; }
    public Enums.EnumViability Viability { get; set; }
    public Enums.EnumLight LightRequirements { get; set; }
    public string WaterRequirements { get; set; }
    public int TemperatureRequirements { get; set; }
}

namespace Server.Controllers.Models;

public class PlantDTO : BaseModelDTO
{
    public Guid ClientId { get; set; }
    public string Specie { get; set; }
    public string Family { get; set; }
    public Enums.EnumFlowers Flower { get; set; }
    public Enums.EnumFruit Fruit { get; set; }
    public Enums.EnumReproduction Reproduction { get; set; }
}

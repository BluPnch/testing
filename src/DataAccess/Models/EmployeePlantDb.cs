namespace DataAccess.Models;

public class EmployeePlantDb
{
    public Guid EmployeeId { get; set; }

    public Guid PlantId { get; set; }
    
    
    public virtual EmployeeDb Employee { get; set; }
    
    public virtual PlantDb Plant { get; set; }
}
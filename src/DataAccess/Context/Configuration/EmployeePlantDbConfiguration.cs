using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;

public class EmployeePlantDbConfiguration : IEntityTypeConfiguration<EmployeePlantDb>
{
    public void Configure(EntityTypeBuilder<EmployeePlantDb> builder)
    {
        builder.HasKey(ep => new { ep.EmployeeId, ep.PlantId });
        
        builder.HasOne(ep => ep.Employee)
            .WithMany(e => e.EmployeePlants)
            .HasForeignKey(ep => ep.EmployeeId);
            
        builder.HasOne(ep => ep.Plant)
            .WithMany(p => p.EmployeePlants)
            .HasForeignKey(ep => ep.PlantId);
    }
}
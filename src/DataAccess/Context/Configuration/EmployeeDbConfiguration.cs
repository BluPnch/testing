using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;

public class EmployeeDbConfiguration : IEntityTypeConfiguration<EmployeeDb>
{
    public void Configure(EntityTypeBuilder<EmployeeDb> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.HasOne(e => e.Administrator)
            .WithMany(a => a.Employees)
            .HasForeignKey(e => e.AdministratorId);
        
        builder.HasMany(e => e.EmployeePlants)
            .WithOne(ep => ep.Employee)
            .HasForeignKey(ep => ep.EmployeeId);
    }
}
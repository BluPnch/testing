using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;


public class PlantDbConfiguration : IEntityTypeConfiguration<PlantDb>
{
    public void Configure(EntityTypeBuilder<PlantDb> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.HasOne(p => p.Client)
            .WithMany(c => c.Plants)
            .HasForeignKey(p => p.ClientId);
        
        builder.HasMany(p => p.Seeds)
            .WithOne(s => s.Plant)
            .HasForeignKey(s => s.PlantId);

        builder.HasMany(p => p.JournalRecords)
            .WithOne(j => j.Plant)
            .HasForeignKey(j => j.PlantId);

        builder.HasMany(p => p.EmployeePlants)
            .WithOne(ep => ep.Plant)
            .HasForeignKey(ep => ep.PlantId);
    }
}
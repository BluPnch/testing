using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;


public class JournalRecordDbConfiguration : IEntityTypeConfiguration<JournalRecordDb>
{
    public void Configure(EntityTypeBuilder<JournalRecordDb> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedOnAdd();
        
        builder.HasOne(j => j.Plant)
            .WithMany(p => p.JournalRecords)
            .HasForeignKey(j => j.PlantId);
            
        builder.HasOne(j => j.GrowthStage)
            .WithMany(g => g.JournalRecords)
            .HasForeignKey(j => j.GrowthStageId);

        builder.HasOne(j => j.Employee)
            .WithMany(e => e.JournalRecords)
            .HasForeignKey(j => j.EmployeeId);
    }
}
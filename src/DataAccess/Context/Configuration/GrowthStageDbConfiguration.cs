using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;


public class GrowthStageDbConfiguration : IEntityTypeConfiguration<GrowthStageDb>
{
    public void Configure(EntityTypeBuilder<GrowthStageDb> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedOnAdd();
        
        builder.HasMany(g => g.JournalRecords)
            .WithOne(j => j.GrowthStage)
            .HasForeignKey(j => j.GrowthStageId);
    }
}
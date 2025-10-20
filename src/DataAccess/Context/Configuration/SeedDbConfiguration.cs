using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;

public class SeedDbConfiguration : IEntityTypeConfiguration<SeedDb>
{
    public void Configure(EntityTypeBuilder<SeedDb> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        
        builder.HasOne(s => s.Plant)
            .WithMany(p => p.Seeds)
            .HasForeignKey(s => s.PlantId);
    }
}
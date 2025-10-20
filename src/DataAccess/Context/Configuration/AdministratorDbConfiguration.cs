using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DataAccess.Context.Configuration;


public class  AdministratorDbConfiguration : IEntityTypeConfiguration<AdministratorDb>
{
    public void Configure(EntityTypeBuilder<AdministratorDb> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();
        
        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(a => a.Username)
            .IsUnique();
        
        builder.HasMany(a => a.Employees)
            .WithOne(e => e.Administrator)
            .HasForeignKey(e => e.AdministratorId);
    }
}
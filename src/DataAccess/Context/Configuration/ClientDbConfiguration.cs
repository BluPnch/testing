using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration;

public class ClientDbConfiguration : IEntityTypeConfiguration<ClientDb>
{
    public void Configure(EntityTypeBuilder<ClientDb> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        
        builder.HasMany(c => c.Plants)
            .WithOne(p => p.Client)
            .HasForeignKey(p => p.ClientId);
    }
}
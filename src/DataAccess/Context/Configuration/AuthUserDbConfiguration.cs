using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configuration
{
    public class AuthUserDbConfiguration : IEntityTypeConfiguration<AuthUserDb>
    {
        public void Configure(EntityTypeBuilder<AuthUserDb> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.Role).IsRequired();
            
            builder.HasIndex(u => u.Username).IsUnique();
        }
    }
} 
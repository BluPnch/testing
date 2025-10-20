using DataAccess.Context.Configuration;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class GreenhouseContext : DbContext
{
    public virtual DbSet<AdministratorDb> Administrators { get; set; }
    public virtual DbSet<EmployeeDb> Employees { get; set; }
    public virtual DbSet<ClientDb> Clients { get; set; }
    public virtual DbSet<PlantDb> Plants { get; set; }
    public virtual DbSet<JournalRecordDb> JournalRecords { get; set; }
    public virtual DbSet<SeedDb> Seeds { get; set; }
    public virtual DbSet<GrowthStageDb> GrowthStages { get; set; }
    public virtual DbSet<EmployeePlantDb> EmployeePlants { get; set; }
    public virtual DbSet<AuthUserDb> AuthUsers { get; set; }

    public GreenhouseContext(DbContextOptions<GreenhouseContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AdministratorDbConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDbConfiguration());
        modelBuilder.ApplyConfiguration(new ClientDbConfiguration());
        modelBuilder.ApplyConfiguration(new PlantDbConfiguration());
        modelBuilder.ApplyConfiguration(new JournalRecordDbConfiguration());
        modelBuilder.ApplyConfiguration(new SeedDbConfiguration());
        modelBuilder.ApplyConfiguration(new GrowthStageDbConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeePlantDbConfiguration());
        modelBuilder.ApplyConfiguration(new AuthUserDbConfiguration());
    }
}
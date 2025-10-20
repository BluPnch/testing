using DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.TestRepositories;

public class RepositoryTestFixture : IDisposable
{
    public GreenhouseContext Context { get; }
    
    public RepositoryTestFixture()
    {
        var options = new DbContextOptionsBuilder<GreenhouseContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        Context = new GreenhouseContext(options);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();
    }
    
    
    public void Dispose()
    {
        Context.Database.CloseConnection();
        Context.Dispose();
    }
}
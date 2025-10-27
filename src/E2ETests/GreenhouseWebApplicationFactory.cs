using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server;
using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace E2ETests;

public class GreenhouseWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Удаляем существующий контекст базы данных
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GreenhouseContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Добавляем контекст базы данных для тестов
            services.AddDbContext<GreenhouseContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Port=5433;Database=GreenhouseContext_Test;Username=postgres;Password=1");
            });

            // Создаем scope для инициализации базы данных
            var sp = services.BuildServiceProvider();
            
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<GreenhouseContext>();
                var logger = scopedServices.GetRequiredService<ILogger<GreenhouseWebApplicationFactory>>();

                try
                {
                    // Обеспечиваем создание базы данных
                    db.Database.EnsureCreated();
                    
                    // Инициализируем тестовые данные
                    DbInitializer.Initialize(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                }
            }
        });
    }
}
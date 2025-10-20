using DataAccess.Models;
using Domain.Models.Enums;
using DefaultNamespace; // Add DefaultNamespace for Enums

namespace DataAccess.Context;

public static class DbInitializer
{
    public static void Initialize(GreenhouseContext context)
    {
        context.Database.EnsureCreated();

        // Проверяем, есть ли уже пользователи
        if (context.AuthUsers.Any())
        {
            return; // База данных уже заполнена
        }

        // Создаем тестового администратора
        var adminId = Guid.NewGuid();
        var adminUser = new AuthUserDb
        {
            Id = adminId,
            Username = "admin@gh.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = EnumAuth.Administrator
        };

        var admin = new AdministratorDb(
            adminId,
            "Admin",
            "Admin",
            "Admin",
            "+7 (999) 999-99-99",
            "admin@gh.com"
        );

        // Создаем тестового сотрудника
        var employeeId = Guid.NewGuid();
        var employeeUser = new AuthUserDb
        {
            Id = employeeId,
            Username = "employee@gh.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
            Role = EnumAuth.Employee
        };

        var employee = new EmployeeDb(
            employeeId,
            "Employee",
            "Employee",
            "Employee",
            "Уход за растениями",
            "Теплица №1",
            "+7 (999) 999-99-98"
        ) { AdministratorId = adminId }; // Привязываем сотрудника к администратору

        // Создаем тестового клиента
        var clientId = Guid.NewGuid();
        var clientUser = new AuthUserDb
        {
            Id = clientId,
            Username = "client@gh.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"),
            Role = EnumAuth.Client
        };

        var client = new ClientDb(
            clientId,
            "Test Company",
            "+7 (999) 999-99-97"
        );

        // Добавляем пользователей и их специфичные данные
        context.AuthUsers.AddRange(adminUser, employeeUser, clientUser);
        context.Administrators.Add(admin);
        context.Employees.Add(employee);
        context.Clients.Add(client);

        // Создаем тестовые растения
        var plant1Id = Guid.NewGuid();
        var plant2Id = Guid.NewGuid();

        var plants = new[]
        {
            new PlantDb(plant1Id, clientId, "Овощная", "Помидор", EnumFlowers.Actinomorphic, EnumFruit.Berry, EnumReproduction.Cutting),
            new PlantDb(plant2Id, clientId, "Фруктовое", "Яблоня", EnumFlowers.Zygomorphic, EnumFruit.Drupe, EnumReproduction.Grafting)
        };
        context.Plants.AddRange(plants);

        // Создаем тестовые семена
        var seeds = new[]
        {
            new SeedDb(Guid.NewGuid(), plant1Id, "Зрелое", EnumViability.Dormant, EnumLight.DirectSunlight, "Умеренный", 20),
            new SeedDb(Guid.NewGuid(), plant2Id, "Недозрелое", EnumViability.FreshlyHarvested, EnumLight.Low, "Высокий", 15)
        };
        context.Seeds.AddRange(seeds);

        // Создаем тестовые этапы роста
        var growthStage1Id = Guid.NewGuid();
        var growthStage2Id = Guid.NewGuid();
        var growthStage3Id = Guid.NewGuid();

        var growthStages = new[]
        {
            new GrowthStageDb(growthStage1Id, "Прорастание", "Начальная стадия роста растения"),
            new GrowthStageDb(growthStage2Id, "Вегетация", "Стадия активного роста листьев и стеблей"),
            new GrowthStageDb(growthStage3Id, "Цветение", "Стадия цветения и образования плодов")
        };
        context.GrowthStages.AddRange(growthStages);

        // Создаем тестовые записи журнала состояний
        var journalRecords = new[]
        {
            new JournalRecordDb(Guid.NewGuid(), 20.5, 3, EnumCondition.Healthy, DateTimeOffset.UtcNow.AddDays(-7), plant1Id, growthStage1Id, employeeId),
            new JournalRecordDb(Guid.NewGuid(), 22.0, 5, EnumCondition.Healthy, DateTimeOffset.UtcNow.AddDays(-3), plant1Id, growthStage2Id, employeeId),
            new JournalRecordDb(Guid.NewGuid(), 150.0, 20, EnumCondition.Healthy, DateTimeOffset.UtcNow.AddDays(-5), plant2Id, growthStage3Id, employeeId)
        };
        context.JournalRecords.AddRange(journalRecords);

        context.SaveChanges();
    }
} 
using DataAccess.Context;
using DataAccess.Repositories;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Application.Services;
using Application.Validators;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog.Events;


namespace Server;

public static class MigrationExtension
{
    public static IServiceCollection Migrate<TContext>(this IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        // Отключаем автоматическое применение миграций
        // var context = serviceCollection.BuildServiceProvider().GetRequiredService<TContext>();
        // context.Database.Migrate();

        return serviceCollection;
    }
}

public static class RepositoryExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddScoped<IAdministratorRepository, AdministratorRepository>();
        service.AddScoped<IEmployeeRepository, EmployeeRepository>();
        service.AddScoped<IClientRepository, ClientRepository>();
        service.AddScoped<IPlantRepository, PlantRepository>();
        service.AddScoped<IJournalRecordRepository, JournalRecordRepository>();
        service.AddScoped<ISeedRepository, SeedRepository>();
        service.AddScoped<IGrowthStageRepository, GrowthStageRepository>();
        service.AddScoped<IAuthUserRepository, AuthUserRepository>();
        return service;
    }
}

public static class ServiceExtension
{
    public static IServiceCollection AddServices(this IServiceCollection service)
    {
        service.AddScoped<IAdministratorService, AdministratorService>();
        service.AddScoped<IEmployeeService, EmployeeService>();
        service.AddScoped<IClientService, ClientService>();
        service.AddScoped<IPlantService, PlantService>();
        service.AddScoped<IJournalRecordService, JournalRecordService>();
        service.AddScoped<ISeedService, SeedService>();
        service.AddScoped<IGrowthStageService, GrowthStageService>();
        service.AddScoped<IAuthService, AuthService>();
        
        service.AddScoped<AdministratorValidator>();
        service.AddScoped<EmployeeValidator>();
        service.AddScoped<ClientValidator>();
        service.AddScoped<PlantValidator>();
        service.AddScoped<JournalRecordValidator>();
        service.AddScoped<SeedValidator>();
        service.AddScoped<GrowthStageValidator>();
        service.AddScoped<AuthUserValidator>();
        
        return service;
    }
}

public static class DbExtension
{
    public static IServiceCollection ConfigureDb(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddDbContext<GreenhouseContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        service.AddRepositories(configuration);
        service.Migrate<GreenhouseContext>();
        return service;
    }
}

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                builder.Configuration["Logging:LogFilePath"] ?? "logs/app.log",
                rollingInterval: Serilog.RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (var fileName in xmlFiles)
            {
                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(xmlFilePath))
                    c.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
            }
            
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Greenhouse API", Version = "v1" });
            
            // Используем полное имя типа (включая namespace) для schemaId, чтобы избежать конфликтов между доменными и DTO enum'ами
            c.CustomSchemaIds(type =>
            {
                if (type.IsGenericType)
                {
                    var genericTypeName = type.GetGenericTypeDefinition().FullName?.Replace("+", ".") ?? type.Name;
                    var genericArgs = string.Join(",", type.GetGenericArguments().Select(arg => arg.FullName?.Replace("+", ".") ?? arg.Name));
                    return $"{genericTypeName}[{genericArgs}]";
                }
                return type.FullName?.Replace("+", ".") ?? type.Name;
            });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
            options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
            options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
        });

        builder.Services.ConfigureDb(builder.Configuration);

        builder.Services.AddServices();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        // Настройка статических файлов
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Настройка маршрутизации для SPA
        app.MapFallbackToFile("index.html");

        app.MapControllers();

        // Инициализация базы данных
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<GreenhouseContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Произошла ошибка при инициализации базы данных.");
            }
        }

        app.Run();
    }
}
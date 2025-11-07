using System.Reflection;
using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Rebus.Config;

namespace InventoryService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(builder =>
            builder
                .UseNpgsql(configuration[Const.PostgresConnectionString]!, o
                    => o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName))
                .LogTo(Console.WriteLine, LogLevel.Information));
        return services;
    }
    
    public static IServiceCollection AddRebus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRebus(setup => setup
            .Logging(c => c.Serilog())
            .Transport(c => c.UseRabbitMq(configuration[Const.RabbitMqConnectionString]!, "inventory-api"))
            .Options(o =>
            {
                o.SetNumberOfWorkers(1);
            }));
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddRequestum(this IServiceCollection services)
    {
        services.AddRequestum(s =>
        {
            s.Default(typeof(Program).Assembly);
            s.RequireEventHandlers = true;
        });
        return services;
    }
}
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure;
using Rebus.Config;

namespace PaymentService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options
            => options.UseNpgsql(configuration["Postgres:ConnectionString"]!, 
                    builder => builder.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName))
                .LogTo(Console.WriteLine, LogLevel.Information));
        return services;
    }
    
    public static IServiceCollection AddRebus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRebus(config =>
        {
            config.Logging(cfg => cfg.Serilog());
            config.Transport(cfg => cfg.UseRabbitMq(configuration["Rabbit:ConnectionString"]!,
                "payment-api"));
            config.Options(options =>
            {
                options.SetNumberOfWorkers(1);
            });
            return config;
        });
        return services;
    }
}
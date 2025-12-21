using Contracts.Messages.Events;
using InventoryWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;
using Serilog;
using Shared.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("Serilog.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .UseSharedSerilog()
    // .UseSerilog((context, services, configuration) => configuration
    //     .ReadFrom.Configuration(context.Configuration)
    //     .ReadFrom.Services(services)
    //     .Enrich.FromLogContext())
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(Random.Shared);
        services.AutoRegisterHandlersFromAssemblyOf<OrderCreatedHandler>();
        services.AddRebus((configure, provider) =>
            configure
                .Logging(l => l.Serilog())
                .Transport(t => t.UseRabbitMq(
                    provider.GetRequiredService<IConfiguration>()["Rabbit:ConnectionString"]!, 
                    "inventory-worker")
                )
                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    o.SetMaxParallelism(4);
                }));
    })
    .Build();

try
{
    await host.StartAsync();

    using var scope = host.Services.CreateScope();
    var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    await bus.Subscribe<OrderCreated>();

    await host.WaitForShutdownAsync();
}
finally
{
    Log.CloseAndFlush();
}
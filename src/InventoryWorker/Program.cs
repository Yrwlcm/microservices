using Contracts.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Serilog;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("Serilog.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext())
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(Random.Shared);
        services.AutoRegisterHandlersFromAssemblyOf<OrderCreatedHandler>();
        services.AddRebus((configure, provider) =>
            configure
                .Logging(l => l.Serilog())
                .Transport(t => t.UseRabbitMq(provider.GetRequiredService<IConfiguration>()["Rabbit:ConnectionString"]!, "inventory-worker"))
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

internal sealed class OrderCreatedHandler(ILogger<OrderCreatedHandler> logger, Random random)
    : IHandleMessages<OrderCreated>
{
    public async Task Handle(OrderCreated message)
    {
        logger.LogInformation("Received order {OrderId} with {ItemCount} item(s)", message.OrderId, message.Items.Count);

        foreach (var item in message.Items)
        {
            logger.LogInformation(" -> SKU {Sku}, qty {Qty}", item.Sku, item.Qty);
        }

        var delay = random.Next(100, 301);
        await Task.Delay(delay);

        logger.LogInformation("Order {OrderId} processed in {Delay} ms", message.OrderId, delay);
    }
}

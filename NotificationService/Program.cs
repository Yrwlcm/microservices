using Contracts.Messages.Events;
using NotificationService.Infrastructure.Consumers;
using NotificationService.Infrastructure.Services;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables()
	.AddCommandLine(args);

builder.Host.UseSharedSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<INotificationService, LogNotificationService>();

builder.Services.AddRebus(config => config
	.Logging(l => l.Serilog())
	.Transport(t => t.UseRabbitMq(builder.Configuration["Rabbit:ConnectionString"]!, "notification-service"))
	.Options(o =>
	{
		o.SetNumberOfWorkers(1);
		o.SetMaxParallelism(8);
	}));

builder.Services.AddTransient<IHandleMessages<OrderCreatedNotification>, OrderCreatedConsumer>();
builder.Services.AddTransient<IHandleMessages<StockReservedNotification>, StockReservedConsumer>();
builder.Services.AddTransient<IHandleMessages<PaymentCompletedNotification>, PaymentCompletedConsumer>();
builder.Services.AddTransient<IHandleMessages<OrderCompletedNotification>, OrderCompletedConsumer>();
builder.Services.AddTransient<IHandleMessages<OrderFailedNotification>, OrderFailedConsumer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

await SubscribeToEvents(app.Services);

app.Run();
return;

static async Task SubscribeToEvents(IServiceProvider services)
{
	using var scope = services.CreateScope();
	var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    
	await bus.Subscribe<OrderCreatedNotification>();
	await bus.Subscribe<StockReservedNotification>();
	await bus.Subscribe<PaymentCompletedNotification>();
	await bus.Subscribe<OrderCompletedNotification>();
	await bus.Subscribe<OrderFailedNotification>();
    
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
	logger.LogInformation("NotificationService subscribed to all events");
}
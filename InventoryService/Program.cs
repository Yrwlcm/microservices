using System.Reflection;
using Contracts.Messages.Requests;
using InventoryService;
using InventoryService.Extensions;
using InventoryService.Infrastructure;
using InventoryService.Infrastructure.Consumers;
using Microsoft.OpenApi.Models;
using Outbox.Services;
using Prometheus;
using Rebus.Handlers;
using Serilog;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Inventory Service API",
        Version = "v1"
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    o.IncludeXmlComments(xmlPath);
});
builder.Services.AddInventoryDbContext(builder.Configuration);
builder.Host.UseSharedSerilog();
builder.Services.AddServices();
builder.Services.AddRequestum();

builder.Services.AddTransient<IHandleMessages<ReleaseStockRequest>, ReleaseStockRequestsConsumer>();
builder.Services.AddTransient<IHandleMessages<ReserveStockRequest>, ReserveStockRequestsConsumer>();

builder.Services.AddHostedService<OutboxProcessor<InventoryDbContext>>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRebus(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseHttpMetrics();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API v1");
    options.RoutePrefix = "swagger";
});
app.UseSerilogRequestLogging();

MigrationsRunner.ApplyMigrations(app.Services);
await BusSubscriber.SubscribeToMessagesAsync(app.Services);
app.MapControllers();
app.MapMetrics();
app.Run();

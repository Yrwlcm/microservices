using System.Reflection;
using InventoryService;
using InventoryService.Extensions;
using InventoryService.Infrastructure;
using Microsoft.OpenApi.Models;
using Outbox.Services;
using Rebus.Config;
using Serilog;

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
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
builder.Services.AddRebus(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddRequestum();
builder.Services.AddHostedService<OutboxProcessor<InventoryDbContext>>();
var app = builder.Build();
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
app.Run();

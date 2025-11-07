using InventoryService;
using InventoryService.Extensions;
using Rebus.Config;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInventoryDbContext(builder.Configuration);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
builder.Services.AddRebus(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddRequestum();
var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

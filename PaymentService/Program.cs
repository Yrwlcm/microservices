using System.Reflection;
using Microsoft.OpenApi.Models;
using PaymentService;
using PaymentService.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
builder.Services.AddControllers();
builder.Services.AddPaymentDbContext(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Payment Service API",
        Version = "v1"
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    setup.IncludeXmlComments(xmlPath);
});
builder.Services.AddRequestum(setup =>
{
    setup.Default(typeof(Program).Assembly);
    setup.RequireEventHandlers = true;
});
builder.Services.AddRebus(builder.Configuration);
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(setup => 
{
setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
setup.RoutePrefix = "swagger";
});
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
MigrationsRunner.ApplyMigrations(app.Services);
app.Run();

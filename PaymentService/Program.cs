using System.Reflection;
using Microsoft.OpenApi.Models;
using Outbox.Services;
using PaymentService;
using PaymentService.Extensions;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Consumers;
using Prometheus;
using Rebus.Config;
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
    setup.Lifetime = ServiceLifetime.Scoped;
    setup.Default(typeof(Program).Assembly);
    setup.RequireEventHandlers = true;
});
builder.Services.AutoRegisterHandlersFromAssemblyOf<PaymentRequestsConsumer>();
builder.Services.AddRebus(builder.Configuration);
builder.Services.AddHostedService<OutboxProcessor<PaymentDbContext>>();
var app = builder.Build();

app.UseRouting();
app.UseHttpMetrics();

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
await BusSubscriber.SubscribeToMessagesAsync(app.Services);

app.MapMetrics();

app.Run();

using OrderService.Models.Orders;
using Rebus.Config;
using Serilog;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("Serilog.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSharedSerilog();

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OrderRequestValidator>();
builder.Services.AddSingleton<OrderMessageFactory>();
builder.Services.AddScoped<IOrderPublisher, OrderPublisher>();

var isTesting = builder.Environment.IsEnvironment("Testing");

if (!isTesting)
{
    builder.Services.AddRebus(config => config
        .Logging(l => l.Serilog())
        .Transport(t => t.UseRabbitMq(builder.Configuration["Rabbit:ConnectionString"]!, "orders-api"))
        .Options(o =>
        {
            o.SetNumberOfWorkers(1);
            o.SetMaxParallelism(8);
        }));
}

var app = builder.Build();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();
app.MapHealthChecks("/healthz");

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();

public partial class Program;

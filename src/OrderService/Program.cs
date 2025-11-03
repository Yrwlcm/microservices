using System.ComponentModel.DataAnnotations;
using Contracts.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.ServiceProvider;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("Serilog.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRebus(config => config
    .Logging(l => l.Serilog())
    .Transport(t => t.UseRabbitMq(builder.Configuration["Rabbit:ConnectionString"]!, "orders-api"))
    .Options(o =>
    {
        o.SetNumberOfWorkers(1);
        o.SetMaxParallelism(8);
    }));

var app = builder.Build();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
    options.RoutePrefix = "swagger";
});

app.MapPost("/order", async (OrderRequest request, IBus bus, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var orderCreated = new OrderCreated(
        request.OrderId,
        request.Items.Select(i => new OrderItem(i.Sku, i.Qty)).ToList());

    var logger = loggerFactory.CreateLogger("OrderPublisher");
    logger.LogInformation("Publishing order {@OrderId} with {ItemCount} item(s)", request.OrderId, orderCreated.Items.Count);

    cancellationToken.ThrowIfCancellationRequested();
    await bus.Publish(orderCreated);

    return Results.Accepted($"/order/{request.OrderId}", new { request.OrderId });
})
.WithName("CreateOrder")
.Produces(StatusCodes.Status202Accepted)
.ProducesValidationProblem()
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/healthz", () => Results.Ok());

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();

static List<string> Validate(OrderRequest request)
{
    var errors = new List<string>();

    if (request.OrderId == Guid.Empty)
    {
        errors.Add("orderId must be a non-empty GUID.");
    }

    if (request.Items is null || request.Items.Count == 0)
    {
        errors.Add("items must contain at least one entry.");
        return errors;
    }

    for (var index = 0; index < request.Items.Count; index++)
    {
        var item = request.Items[index];
        if (string.IsNullOrWhiteSpace(item.Sku))
        {
            errors.Add($"items[{index}].sku must be provided.");
        }

        if (item.Qty <= 0)
        {
            errors.Add($"items[{index}].qty must be greater than zero.");
        }
    }

    return errors;
}

internal sealed record OrderRequest
{
    [Required]
    public Guid OrderId { get; init; }

    [Required]
    public List<OrderLineRequest> Items { get; init; } = new();
}

internal sealed record OrderLineRequest
{
    [Required]
    public string Sku { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Qty { get; init; }
}

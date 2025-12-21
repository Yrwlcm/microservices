using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Shared.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseSharedSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            var serviceName = context.HostingEnvironment.ApplicationName;
            var environment = context.HostingEnvironment.EnvironmentName;

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.WithProperty("Service", serviceName)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithCorrelationId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {Service} {CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(context.Configuration.GetValue<string>("Seq:Url") ?? "http://seq:5341");
        });
    }

    public static IServiceCollection AddSharedOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new List<KeyValuePair<string, object>>
                {
                    new("deployment.environment", "development"),
                    new("service.version", "1.0.0")
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(serviceName)
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(serviceName)
                .AddPrometheusExporter());

        return services;
    }
}
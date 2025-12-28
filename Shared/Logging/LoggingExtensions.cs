using Microsoft.Extensions.Hosting;
using Serilog;

namespace Shared.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseSharedSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, _, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithProperty("Service", context.HostingEnvironment.ApplicationName)
                .WriteTo.Console()
                .WriteTo.Seq("http://seq:5341");
        });
    }
}
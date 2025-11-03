using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Rebus.Bus;

namespace OrderService.Tests;

public class OrdersApiApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var busDescriptors = services.Where(d => d.ServiceType == typeof(IBus)).ToList();
            foreach (var descriptor in busDescriptors)
            {
                services.Remove(descriptor);
            }

            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService) &&
                            d.ImplementationType?.Namespace?.StartsWith("Rebus", StringComparison.Ordinal) == true)
                .ToList();

            foreach (var descriptor in hostedServices)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton(_ =>
            {
                var bus = Substitute.For<IBus>();
                bus.Publish(Arg.Any<object>()).Returns(Task.CompletedTask);
                bus.Publish(Arg.Any<object>(), Arg.Any<IDictionary<string, string>>()).Returns(Task.CompletedTask);
                return bus;
            });
        });

        return base.CreateHost(builder);
    }
}
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Contracts.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NUnit.Framework;
using Rebus.Bus;

namespace OrderService.Tests;

[TestFixture]
public sealed class OrderEndpointTests
{
    private OrdersApiApplicationFactory? factory;

    [SetUp]
    public void SetUp()
    {
        factory = new OrdersApiApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        factory?.Dispose();
    }

    [Test]
    public async Task CreateOrder_ValidRequest_ReturnsAcceptedAndPublishesMessage()
    {
        using var client = factory!.CreateClient();

        var response = await client.PostAsJsonAsync("/order", new
        {
            orderId = Guid.NewGuid(),
            items = new[]
            {
                new { sku = "ABC", qty = 2 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var bus = factory.Services.GetRequiredService<IBus>();
        await bus.Received(1).Publish(Arg.Is<OrderCreated>(m => m.Items.Count == 1));
    }

    [Test]
    public async Task CreateOrder_EmptyItems_ReturnsBadRequest()
    {
        using var client = factory!.CreateClient();

        var response = await client.PostAsJsonAsync("/order", new
        {
            orderId = Guid.NewGuid(),
            items = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var bus = factory.Services.GetRequiredService<IBus>();
        await bus.DidNotReceive().Publish(Arg.Any<OrderCreated>());
    }

    [Test]
    public async Task Healthz_ReturnsOk()
    {
        using var client = factory!.CreateClient();

        var response = await client.GetAsync("/healthz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class OrdersApiApplicationFactory : WebApplicationFactory<Program>
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
}

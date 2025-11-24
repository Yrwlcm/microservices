using System.Net;
using System.Net.Http.Json;
using Contracts.Messages.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Rebus.Bus;

namespace OrderService.Tests;

[TestFixture]
public class OrderEndpointTests
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
                new { sku = "ABC", quantity = 2 }
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
}

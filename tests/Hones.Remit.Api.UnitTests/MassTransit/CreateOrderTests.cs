using Hones.Remit.Api.Data;
using Hones.Remit.Api.MassTransit.Events.OrderCreated;
using Hones.Remit.Api.MassTransit.Requests.CreateOrder;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hones.Remit.Api.UnitTests.MassTransit;

public class CreateOrderTests
{
    [Fact]
    public async Task CreateOrder_WhenOrderIsCreated_ShouldPublishOrderCreatedEvent()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddDbContext<OrdersDbContext>(context => { context.UseInMemoryDatabase(Guid.NewGuid().ToString()); })
            .AddMassTransitTestHarness(x =>
            {
                x.AddInMemoryInboxOutbox();
                x.AddConsumer<CreateOrderConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var client = harness.GetRequestClient<CreateOrder>();

        await client.GetResponse<NewOrderResult, OrderCreationFailedResult>(new CreateOrder
        {
            SenderEmail = "sender@example.co",
            Amount = 1000,
            Currency = "USD",
            RecipientEmail = "recipient@example.com",
            SenderName = "Sender",
            RecipientName = "Recipient"
        });

        var messageConsumed = await harness.Consumed.Any<CreateOrder>();
        messageConsumed.Should()
            .BeTrue();
        
        var eventPublished = await harness.Published.Any<OrderCreated>();
        eventPublished.Should()
            .BeTrue();
    }
}
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Commands;

public class CollectOrderConsumer : IConsumer<CollectOrder>
{
    private readonly OrdersDbContext _dbContext;

    public CollectOrderConsumer(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CollectOrder> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            return;
        }

        var result = order.Collect();
        
        if (result.IsError)
        {
            return;
        }

        await context.Publish(new OrderCollected(order.PublicId), context.CancellationToken);
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

public class CollectOrderConsumerDefinition : ConsumerDefinition<CollectOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CollectOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
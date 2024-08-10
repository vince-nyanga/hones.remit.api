using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Commands;

public class CancelOrderConsumer : IConsumer<CancelOrder>
{
    private readonly OrdersDbContext _dbContext;

    public CancelOrderConsumer(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.PublicId == context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            return;
        }
        
        var result = order.Cancel();
        
        if (result.IsError)
        {
            return;
        }
        
        await context.Publish(new OrderCancelled(order.PublicId), context.CancellationToken);
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

public class CancelOrderConsumerDefinition : ConsumerDefinition<CancelOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CancelOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
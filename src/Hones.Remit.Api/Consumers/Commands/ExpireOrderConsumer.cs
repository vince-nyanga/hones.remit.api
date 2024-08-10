using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Commands;

public class ExpireOrderConsumer : IConsumer<ExpireOrder>
{
    private readonly OrdersDbContext _dbContext;

    public ExpireOrderConsumer(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ExpireOrder> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            return;
        }

        var result = order.Expire();
        
        if (result.IsError)
        {
            return;
        }
        
        await context.Publish(new OrderExpired(order.PublicId), context.CancellationToken);
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

public class ExpireOrderConsumerDefinition : ConsumerDefinition<ExpireOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ExpireOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
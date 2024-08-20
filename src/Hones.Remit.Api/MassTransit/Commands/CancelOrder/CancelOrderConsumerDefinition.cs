using Hones.Remit.Api.Data;
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Commands.CancelOrder;

public class CancelOrderConsumerDefinition : ConsumerDefinition<CancelOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CancelOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
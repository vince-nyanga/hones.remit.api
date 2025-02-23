using Hones.Remit.Api.MassTransit.Events.ValidationSucceeded;
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Events.ValidatePayment;

public class ValidatePaymentConsumer(IBus bus) : IConsumer<ValidatePayment>
{
    public async Task Consume(ConsumeContext<ValidatePayment> context)
    {
        // Simulate processing, for instance, running fraud checks
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        // Publishing through IBus since we don't need to go through the outbox
        await bus.Publish(new PaymentValidationSucceeded(context.Message.OrderId), context.CancellationToken);
    }
}
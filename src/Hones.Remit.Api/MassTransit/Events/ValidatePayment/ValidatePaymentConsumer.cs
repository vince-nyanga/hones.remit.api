using Hones.Remit.Api.Data;
using Hones.Remit.Api.MassTransit.Events.ValidationSucceeded;
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Events.ValidatePayment;

public class ValidatePaymentConsumer : IConsumer<ValidatePayment>
{
    private readonly OrdersDbContext _db;

    public ValidatePaymentConsumer(OrdersDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<ValidatePayment> context)
    {
        // Simulate processing, for instance, running fraud checks
        await Task.Delay(TimeSpan.FromSeconds(5));
        await context.Publish(new PaymentValidationSucceeded(context.Message.OrderId), context.CancellationToken);
        
        // since we are using the outbox pattern, we need to save the changes to the database
        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
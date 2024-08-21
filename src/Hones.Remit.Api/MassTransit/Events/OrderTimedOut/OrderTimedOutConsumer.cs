using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.MassTransit.Events.OrderTimedOut;

public class OrderTimedOutConsumer : IConsumer<OrderTimedOut>
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEmailService _emailService;

    public OrderTimedOutConsumer(OrdersDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderTimedOut> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            return;
        }

        order.Expire();
        await _dbContext.SaveChangesAsync();

        // ideally, we should publish an event to that outbox can do its job... We can just be lazy for now.
        var orderReference = order.Id.Encode();
        var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
            .AppendLine()
            .AppendLine()
            .AppendLine("Unfortunately your order has expired.")
            .AppendLine()
            .AppendLine("Order Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.SenderEmail, $"Order Expired - {orderReference}",
            emailBuilder.ToString());
    }
}

public class OrderExpiredConsumerDefinition : ConsumerDefinition<OrderTimedOutConsumer>
{
    public OrderExpiredConsumerDefinition()
    {
        Endpoint(x =>
        {
            x.Name = "order-timed-out-email-sender";
        });
    }
}
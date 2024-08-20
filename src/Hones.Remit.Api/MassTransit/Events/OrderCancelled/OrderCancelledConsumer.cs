using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.MassTransit.Events.OrderCancelled;

public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEmailService _emailService;

    public OrderCancelledConsumer(OrdersDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.PublicId == context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            return;
        }

        var orderReference = order.Id.Encode();
        var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
            .AppendLine()
            .AppendLine()
            .AppendLine("You have successfully cancelled your order.")
            .AppendLine()
            .AppendLine("Order Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.SenderEmail, $"Order Cancelled - {orderReference}",
            emailBuilder.ToString());
    }
}
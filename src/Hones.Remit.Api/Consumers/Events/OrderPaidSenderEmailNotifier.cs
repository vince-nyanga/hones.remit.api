using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Events;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Events;

public class OrderPaidSenderEmailNotifier : IConsumer<OrderPaid>
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEmailService _emailService;

    public OrderPaidSenderEmailNotifier(OrdersDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderPaid> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            return;
        }

        var orderReference = order.Id.Encode();
        var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
            .AppendLine()
            .AppendLine()
            .AppendLine("Thank you for your payment. Your order is now ready for collection.")
            .AppendLine()
            .AppendLine("Order Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.SenderEmail, $"Order ready for collection - {orderReference}",
            emailBuilder.ToString());
    }
}
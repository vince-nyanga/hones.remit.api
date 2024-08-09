using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Events;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Events;

public class OrderCollectedRecipientEmailNotifier : IConsumer<OrderCollected>
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEmailService _emailService;

    public OrderCollectedRecipientEmailNotifier(OrdersDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderCollected> context)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            return;
        }

        var orderReference = order.Id.Encode();
        var emailBuilder = new StringBuilder($"Hi {order.RecipientName},")
            .AppendLine()
            .AppendLine()
            .AppendLine($"You have successfully collected the money sent by {order.SenderName}.")
            .AppendLine()
            .AppendLine("Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Sender: {order.SenderName} ({order.SenderEmail})")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.RecipientEmail, $"Money collected  - {orderReference}",
            emailBuilder.ToString());
    }
}
using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.MassTransit.Events.OrderReadyForCollection;

public class OrderReadyForCollectionRecipientEmailNotifier : IConsumer<OrderReadyForCollection>
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEmailService _emailService;

    public OrderReadyForCollectionRecipientEmailNotifier(OrdersDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderReadyForCollection> context)
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
            .AppendLine(
                $"{order.SenderName} has sent you some money. Please go to your nearest HonesRemit collection point to collect.")
            .AppendLine()
            .AppendLine("Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Sender: {order.SenderName} ({order.SenderEmail})")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.RecipientEmail, $"You have received some money  - {orderReference}",
            emailBuilder.ToString());
    }
}
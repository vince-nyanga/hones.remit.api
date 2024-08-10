using System.Text;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.MassTransit.Events.OrderCreated;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly OrdersDbContext _dbContext;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IEmailService _emailService;

    public OrderCreatedConsumer(
        OrdersDbContext dbContext, 
        ILogger<OrderCreatedConsumer> logger, 
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        _logger.LogInformation("Order created: {@Order}", context.Message);

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            _logger.LogError("Order not found: {OrderId}", context.Message.OrderId);
            return;
        }

        var orderReference = order.Id.Encode();
        var emailBuilder = new StringBuilder($"Hi {order.SenderName},")
            .AppendLine()
            .AppendLine()
            .AppendLine("Your order has been created successfully. Please make payment to complete the order.")
            .AppendLine()
            .AppendLine("Order Details:")
            .AppendLine($"Amount: {order.Currency} {order.Amount:N2}")
            .AppendLine($"Reference: {orderReference}")
            .AppendLine($"Recipient: {order.RecipientName} ({order.RecipientEmail})")
            .AppendLine()
            .AppendLine("Thank you for using our service.")
            .AppendLine()
            .AppendLine("Regards,")
            .AppendLine("HonesRemit Team");

        await _emailService.SendEmailAsync(order.SenderEmail, $"Order Created - {orderReference}",
            emailBuilder.ToString());

        _logger.LogInformation("Order created email sent: {OrderId}", context.Message.OrderId);
    }
}
using Hones.Remit.Api.Data;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Consumers.Commands;

public class PayOrderConsumer : IConsumer<PayOrder>
{
    private readonly OrdersDbContext _dbContext;
    private readonly ILogger<PayOrderConsumer> _logger;

    public PayOrderConsumer(OrdersDbContext dbContext, ILogger<PayOrderConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PayOrder> context)
    {
        _logger.LogInformation("Paying order {OrderId}", context.Message.OrderId);
        
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == context.Message.OrderId,
                context.CancellationToken);

        if (order is null)
        {
            _logger.LogError("Order not found: {OrderId}", context.Message.OrderId);
            return;
        }

        var result = order.Pay();

        if (!result.IsError)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Order paid: {OrderId}", context.Message.OrderId);
            
            await context.Publish(new OrderPaid(order.PublicId));
            return;
        }
        
        _logger.LogError("Order payment failed: {OrderId}", context.Message.OrderId);
        // TODO: publish OrderPaymentFailed event
    }
}
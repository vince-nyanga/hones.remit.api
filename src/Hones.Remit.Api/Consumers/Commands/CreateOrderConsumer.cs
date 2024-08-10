using Hones.Remit.Api.Data;
using Hones.Remit.Api.Domain;
using Hones.Remit.Api.Messages.Commands;
using Hones.Remit.Api.Messages.Events;
using Hones.Remit.Api.Messages.Results;
using MassTransit;

namespace Hones.Remit.Api.Consumers.Commands;

public class CreateOrderConsumer : IConsumer<CreateOrder>
{
    private readonly ILogger<CreateOrderConsumer> _logger;
    private readonly OrdersDbContext _dbContext;

    public CreateOrderConsumer(
        ILogger<CreateOrderConsumer> logger, 
        OrdersDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CreateOrder> context)
    {
        _logger.LogInformation("Creating order: {@Order}", context.Message);
        
        var orderResult = Order.Create(
            context.Message.SenderEmail,
            context.Message.SenderName,
            context.Message.RecipientEmail,
            context.Message.RecipientName,
            context.Message.Currency,
            context.Message.Amount
        );

        if (orderResult.IsError)
        {
            await context.RespondAsync(new OrderCreationFailedResult
            {
                Error = orderResult.FirstError.Description
            });
            
            return;
        }
        
        var order = orderResult.Value;

        await _dbContext.Orders.AddAsync(order, context.CancellationToken);
        
        await context.RespondAsync(new NewOrderResult
        {
            Id = order.PublicId,
            Reference = order.Id.Encode(),
            Status = order.Status.ToString(),
            DateCreatedUtc = order.DateCreatedUtc,
            SenderEmail = order.SenderEmail,
            SenderName = order.SenderName,
            RecipientEmail = order.RecipientEmail,
            RecipientName = order.RecipientName,
            Currency = order.Currency,
            Amount = order.Amount
        });

        await  context.Publish(new OrderCreated(order.PublicId), context.CancellationToken);
        
         await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Order created: {@Order}", order);
    }
}

public class CreateOrderConsumerDefinition : ConsumerDefinition<CreateOrderConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CreateOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<OrdersDbContext>(context);
    }
}
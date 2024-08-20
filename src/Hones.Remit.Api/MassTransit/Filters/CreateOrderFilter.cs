using Hones.Remit.Api.MassTransit.Requests.CreateOrder;
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Filters;

public class CreateOrderFilter : IFilter<SendContext<CreateOrder>>
{
    private readonly ILogger<CreateOrderFilter> _logger;

    public CreateOrderFilter(ILogger<CreateOrderFilter> logger)
    {
        _logger = logger;
    }

    public Task Send(SendContext<CreateOrder> context, IPipe<SendContext<CreateOrder>> next)
    {
        _logger.LogInformation("Creating order: {Amount} {Currency}", 
            context.Message.Amount, context.Message.Currency);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Filters;

public class ConsumeLoggerFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<ConsumeLoggerFilter<T>> _logger;

    public ConsumeLoggerFilter(ILogger<ConsumeLoggerFilter<T>> logger)
    {
        _logger = logger;
    }

    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogInformation("Consuming message: {@Message}", context.Message);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
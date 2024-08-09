using MassTransit;

namespace Hones.Remit.Api.Filters;

public class SendLoggerFilter<T> : IFilter<SendContext<T>> where T : class
{
    private readonly ILogger<SendLoggerFilter<T>> _logger;

    public SendLoggerFilter(ILogger<SendLoggerFilter<T>> logger)
    {
        _logger = logger;
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        _logger.LogInformation("Sending message: {@Message}", context.Message);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
using MassTransit;

namespace Hones.Remit.Api.MassTransit.Filters;

public class PublishLoggerFilter<T> : IFilter<PublishContext<T>> where T : class
{
    private readonly ILogger<PublishLoggerFilter<T>> _logger;

    public PublishLoggerFilter(ILogger<PublishLoggerFilter<T>> logger)
    {
        _logger = logger;
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        _logger.LogInformation("Publishing message: {@Message}", context.Message);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
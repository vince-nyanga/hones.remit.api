namespace Hones.Remit.Api.MassTransit.Requests.CreateOrder;

public record OrderCreationFailedResult
{
    public required string Error { get; init; }
}
namespace Hones.Remit.Api.Messages.Results;

public record OrderCreationFailedResult
{
    public required string Error { get; init; }
}
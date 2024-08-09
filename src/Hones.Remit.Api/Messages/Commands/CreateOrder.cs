namespace Hones.Remit.Api.Messages.Commands;

public record CreateOrder
{
    public required string SenderEmail { get; init; }
    public required string SenderName { get; init; } 
    public required string RecipientEmail { get; init; }
    public required string RecipientName { get; init; }
    public required string Currency { get; init; }
    public required decimal Amount { get; init; }
}
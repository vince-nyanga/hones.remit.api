namespace Hones.Remit.Api.MassTransit.Requests.CreateOrder;

public record NewOrderResult
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset DateCreatedUtc { get; init; }
    public DateTimeOffset? DateExpiredUtc { get; init; }
    public DateTimeOffset? DatePaidUtc { get; init; }
    public DateTimeOffset? DateCancelledUtc { get; init; }
    public DateTimeOffset? DateCollectedUtc { get; init; }
    public required string SenderEmail { get; init; }
    public required string SenderName { get; init; } 
    public required string RecipientEmail { get; init; }
    public required string RecipientName { get; init; }
    public required string Currency { get; init; }
    public decimal Amount { get; init; } 
}
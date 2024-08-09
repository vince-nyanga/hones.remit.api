using ErrorOr;
using Hones.Remit.Api.Common.Enums;
using Hones.Remit.Api.Domain.Errors;

namespace Hones.Remit.Api.Domain;

public class Order
{
    private static readonly string[] SupportedCurrencies = ["ZAR", "USD", "ZIG"];
    
    private Order()
    {
    }
    
    private Order(
        Guid publicId,
        string senderEmail, 
        string senderName,
        string recipientEmail, 
        string recipientName, 
        string currency, 
        decimal amount)
    {
        PublicId = publicId;
        SenderEmail = senderEmail;
        SenderName = senderName;
        RecipientEmail = recipientEmail;
        RecipientName = recipientName;
        Currency = currency;
        Amount = amount;
        Status = OrderStatus.Created;
        DateCreatedUtc = DateTimeOffset.UtcNow;
    }
    
    public long Id { get; private set; }
    public Guid PublicId { get; init; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset DateCreatedUtc { get; init; }
    public DateTimeOffset? DateExpiredUtc { get; private set; }
    public DateTimeOffset? DatePaidUtc { get; private set; }
    public DateTimeOffset? DateCancelledUtc { get; private set; }
    public DateTimeOffset? DateCollectedUtc { get; private set; }
    public string SenderEmail { get; init; }
    public string SenderName { get; init; } 
    public string RecipientEmail { get; init; }
    public string RecipientName { get; init; }
    public string Currency { get; init; }
    public decimal Amount { get; init; }


    public static ErrorOr<Order> Create(
        string senderEmail,
        string senderName,
        string recipientEmail,
        string recipientName,
        string currency,
        decimal amount,
        Guid? publicId = default)
    {
        var validationErrors = new List<Error>();
        if (!SupportedCurrencies.Contains(currency))
        {
           validationErrors.Add(OrderErrors.InvalidCurrency);
        }

        if (amount <= 0)
        {
            validationErrors.Add(OrderErrors.AmountMustBePositive);
        }

        if (string.IsNullOrEmpty(senderEmail))
        {
            validationErrors.Add(OrderErrors.SenderEmailRequired);
        }

        if (string.IsNullOrEmpty(senderName))
        {
            validationErrors.Add(OrderErrors.SenderNameRequired);
        }

        if (string.IsNullOrEmpty(recipientEmail))
        {
            validationErrors.Add(OrderErrors.RecipientEmailRequired);
        }

        if (string.IsNullOrEmpty(recipientName))
        {
            validationErrors.Add(OrderErrors.RecipientNameRequired);
        }

        if (validationErrors.Count > 0)
        {
            return validationErrors;
        }

        return new Order(
            publicId ?? Guid.NewGuid(),
            senderEmail,
            senderName,
            recipientEmail,
            recipientName,
            currency,
            amount
        );
    }
}
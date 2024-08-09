using ErrorOr;

namespace Hones.Remit.Api.Domain.Errors;

internal static class OrderErrors
{
    public static Error InvalidCurrency = Error.Validation(description: "Currency not supported");
    public static Error AmountMustBePositive = Error.Validation(description: "Amount must be greater than zero");
    public static Error SenderEmailRequired = Error.Validation(description: "Sender email is required");
    public static Error SenderNameRequired = Error.Validation(description: "Sender name is required");
    public static Error RecipientEmailRequired = Error.Validation(description: "Recipient email is required");
    public static Error RecipientNameRequired = Error.Validation(description: "Recipient name is required");
}
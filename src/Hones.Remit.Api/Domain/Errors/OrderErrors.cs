using ErrorOr;

namespace Hones.Remit.Api.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error UnsupportedCurrency = Error.Validation(description: "Currency not supported");
    public static readonly Error AmountMustBePositive = Error.Validation(description: "Amount must be greater than zero");
    
    public static readonly Error SenderEmailRequired = Error.Validation(description: "Sender email is required");
    public static readonly Error SenderNameRequired = Error.Validation(description: "Sender name is required");
    
    public static readonly Error RecipientEmailRequired = Error.Validation(description: "Recipient email is required");
    public static readonly Error RecipientNameRequired = Error.Validation(description: "Recipient name is required");
    
    public static Error InvalidStatus  = Error.Validation(description: "Operation not allowed on order with current status");
    
    // TODO: more errors can be added here e.g. validation error on field lengths...
}
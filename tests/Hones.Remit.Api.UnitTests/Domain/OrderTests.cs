using Hones.Remit.Api.Common.Enums;
using Hones.Remit.Api.Domain;
using Hones.Remit.Api.Domain.Errors;

namespace Hones.Remit.Api.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithInvalidCurrency_ThrowsException()
    {
        // Arrange
        var currency = "GBP";
        var amount = 100.0m;
        var senderEmail = "sender@email.com";
        var senderName = "Sender Name";
        var recipientEmail = "recipient@email.com";
        var recipientName = "Recipient Name";
        
        // Act
        var result = Order.Create(senderEmail, senderName, recipientEmail, recipientName, currency, amount);
        
        // Assert
        result.IsError
            .Should()
            .BeTrue();

        result.FirstError
            .Should()
            .Be(OrderErrors.UnsupportedCurrency);
    }

    [Fact]
    public void Create_WithInvalidAmount_ThrowsException()
    {
        // Arrange
        var currency = "USD";
        var amount = 0.0m;
        var senderEmail = "sender@email.com";
        var senderName = "Sender Name";
        var recipientEmail = "recipient@email.com";
        var recipientName = "Recipient Name";

        // Act
        var result = Order.Create(senderEmail, senderName, recipientEmail, recipientName, currency, amount);

        // Assert
        result.IsError
            .Should()
            .BeTrue();

        result.FirstError
            .Should()
            .Be(OrderErrors.AmountMustBePositive);
    }
    
   // TODO: Add more tests for other validation rules


   [Fact]
   public void Create_WithValidData_CreatesOrder()
   {
       var currency = "USD";
       var amount = 100.0m;
       var senderEmail = "sender@email.com";
       var senderName = "Sender Name";
       var recipientEmail = "recipient@email.com";
       var recipientName = "Recipient Name";

       // Act
       var result = Order.Create(senderEmail, senderName, recipientEmail, recipientName, currency, amount);

       // Assert
       result.IsError
           .Should()
           .BeFalse();

       var order = result.Value;
       order.Amount
           .Should()
           .Be(amount);

       order.Currency
           .Should()
           .Be(currency);

       order.SenderEmail
           .Should()
           .Be(senderEmail);

       order.SenderName
           .Should()
           .Be(senderName);

       order.RecipientEmail
           .Should()
           .Be(recipientEmail);

       order.RecipientName
           .Should()
           .Be(recipientName);

       order.Status
           .Should()
           .Be(OrderStatus.Created);
   }
   
   // TODO: Add more tests for other business rules :)
}
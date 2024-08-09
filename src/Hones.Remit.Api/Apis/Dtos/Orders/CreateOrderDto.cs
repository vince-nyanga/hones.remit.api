using System.ComponentModel.DataAnnotations;

namespace Hones.Remit.Api.Apis.Dtos.Orders;

public class CreateOrderDto
{
    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public required string SenderEmail { get; init; }
    
    [Required]
    [MaxLength(100)]
    public required string SenderName { get; init; } 
    
    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public required string RecipientEmail { get; init; }
    
    [Required]
    [MaxLength(100)]
    public required string RecipientName { get; init; }
    
    [Required]
    [MaxLength(3)]
    public required string Currency { get; init; }
    
    [Required]
    [Range(1, double.MaxValue)]
    public required decimal Amount { get; init; }
}
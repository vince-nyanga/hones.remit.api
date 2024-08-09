using Hones.Remit.Api.Common.Enums;
using Hones.Remit.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hones.Remit.Api.Data.EntityConfigurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Amount);
        
        builder.Property(x => x.DateCreatedUtc)
            .IsRequired();
        
        builder.Property(x => x.DateExpiredUtc);
        
        builder.Property(x => x.DatePaidUtc);
        
        builder.Property(x => x.DateCancelledUtc);
        
        builder.Property(x => x.DateCollectedUtc);
        
        builder.Property(x => x.SenderEmail)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SenderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RecipientName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(orderStatus => orderStatus.ToString(),
                value => Enum.Parse<OrderStatus>(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();
    }
}
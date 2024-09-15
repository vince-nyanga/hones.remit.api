using Hones.Remit.Api.MassTransit.Sagas.StateMachine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hones.Remit.Api.Data.EntityConfigurations;

public class OrderStateEntityTypeConfiguration : IEntityTypeConfiguration<OrderState>
{
    public void Configure(EntityTypeBuilder<OrderState> builder)
    {
        builder.ToTable("order_states");

        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.CurrentState)
            .HasMaxLength(50);
    }
}
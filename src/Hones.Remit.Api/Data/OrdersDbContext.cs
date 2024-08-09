using Hones.Remit.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hones.Remit.Api.Data;

public class OrdersDbContext : DbContext
{
    private const string Schema = "remit";
    
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
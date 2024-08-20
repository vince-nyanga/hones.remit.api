using System.Reflection;
using Hones.Remit.Api.Data;
using Hones.Remit.Api.MassTransit.Filters;
using Hones.Remit.Api.MassTransit.Sagas.StateMachine;
using MassTransit;

namespace Hones.Remit.Api.MassTransit;

internal static class BuilderExtensions
{
    public static void ConfigureMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(configurator =>
        {
            configurator.AddEntityFrameworkOutbox<OrdersDbContext>(x =>
            {
                x.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
                // there are other configuration options available
                x.UsePostgres();
                x.UseBusOutbox();
            });

            configurator.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(repo =>
                {
                    repo.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    repo.ExistingDbContext<OrdersDbContext>();
                });
    
            configurator.SetKebabCaseEndpointNameFormatter();
    
            var entryAssembly = Assembly.GetEntryAssembly();
            configurator.AddConsumers(entryAssembly);
    
            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", 9520, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
        
                cfg.UseSendFilter<CreateOrderFilter>(context);
                cfg.UseSendFilter(typeof(SendLoggerFilter<>), context);
                cfg.UsePublishFilter(typeof(PublishLoggerFilter<>), context);
                cfg.UseConsumeFilter(typeof(ConsumeLoggerFilter<>), context);
        
                cfg.ConfigureEndpoints(context);
            });

        });
    }
}
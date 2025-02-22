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
            configurator.SetKebabCaseEndpointNameFormatter();
            var entryAssembly = Assembly.GetEntryAssembly();
            configurator.AddConsumers(entryAssembly);
            
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
            
            configurator.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host("us-east-2", h =>
                {
                    h.AccessKey(builder.Configuration["Aws:AccessKey"]);
                    h.SecretKey(builder.Configuration["Aws:SecretKey"]);
                    
                    h.Scope("remittance-dev", scopeTopics: true);

                });
                
                cfg.UseInMemoryScheduler();
        
                cfg.UseSendFilter<CreateOrderFilter>(context);
                cfg.UseSendFilter(typeof(SendLoggerFilter<>), context);
                cfg.UsePublishFilter(typeof(PublishLoggerFilter<>), context);
                cfg.UseConsumeFilter(typeof(ConsumeLoggerFilter<>), context);
                
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("remittance-dev"));
            });

        });
    }
}
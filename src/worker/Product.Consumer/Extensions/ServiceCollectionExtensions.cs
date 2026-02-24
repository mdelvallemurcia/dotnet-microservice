using MassTransit;
using ProjectSubscriber.MassTransit;
using ProjectSubscriber.Options;
using System.Reflection;

namespace ProjectSubscriber.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services, RabbitMqOptions rabbitMqOptions)
    {
        services
            .AddMassTransit(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConnectConsumeObserver(new ConsumeObserver());

                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.Vhost, h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                    });
                   
                    cfg.ReceiveEndpoint(
                        "pm.subscriber",
                        e =>
                        {
                            //e.ConfigureConsumeTopology = false; // para evitar que se creen colas y bindings automáticamente
                            e.SetQuorumQueue();                 // ojo! posible procesamiento duplicado, pero garantiza alta disponibilidad y durabilidad                        
                            e.PrefetchCount = 32;               // cuántos mensajes se bajan a la RAM de la App a la vez
                            e.ConcurrentMessageLimit = 10;      // hilos

                            //e.Bind(
                            //    rabbitMqOptions.ExchangeName, 
                            //    s => {
                            //        s.ExchangeType = ExchangeType.Topic;
                            //        s.RoutingKey = rabbitMqOptions.RoutingKeyFilter;
                            //    }
                            //);

                            // Configurar consumers automáticamente después de los binds
                            e.ConfigureConsumers(context);
                        }
                    );

                    cfg.UseMessageRetry(r =>
                    {
                        r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                        //r.Ignore<InvalidOperationException>();
                    });

                    cfg.UseKillSwitch(options =>
                        options
                            .SetActivationThreshold(10)
                            .SetTripThreshold(0.15)
                            .SetRestartTimeout(TimeSpan.FromSeconds(30))
                            .SetTrackingPeriod(TimeSpan.FromSeconds(30))
                    );
                                        
                });

            });

        return services;
    }
}

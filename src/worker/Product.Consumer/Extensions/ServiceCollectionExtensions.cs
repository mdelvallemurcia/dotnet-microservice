using MassTransit;
using Models.Events;
using ProjectSubscriber.MassTransit;
using ProjectSubscriber.Options;
using RabbitMQ.Client;
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
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.Vhost, h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                    });

                    // Configurar EntityNameFormatter ANTES de los ReceiveEndpoints para que los binds usen el mismo formato
                    cfg.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter());

                    cfg.ReceiveEndpoint(nameof(ProjectSubscriber), e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.SetQuorumQueue();             // ojo! posible procesamiento duplicado, pero garantiza alta disponibilidad y durabilidad                        
                        e.PrefetchCount = 32;           // cuántos mensajes se bajan a la RAM de la App a la vez
                        e.ConcurrentMessageLimit = 10;  // hilos

                        // Vincular cada consumer con su tipo de mensaje usando el FullName
                        // Esto debe hacerse ANTES de ConfigureConsumers cuando ConfigureConsumeTopology = false
                        var consumerTypes = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => t.GetInterfaces().Any(i => 
                                i.IsGenericType && 
                                i.GetGenericTypeDefinition() == typeof(IConsumer<>) &&
                                typeof(IEvent).IsAssignableFrom(i.GetGenericArguments()[0])))
                            .ToList();

                        foreach (var consumerType in consumerTypes)
                        {
                            var messageType = consumerType.GetInterfaces()
                                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
                                .GetGenericArguments()[0];
                            
                            var fullName = messageType.FullName;
                            if (!string.IsNullOrEmpty(fullName))
                            {
                                e.Bind(fullName, s =>
                                {
                                    s.ExchangeType = ExchangeType.Topic;
                                    s.RoutingKey = fullName; // Usar el FullName como routing key para coincidir con el publicado
                                });
                            }
                        }

                        // Configurar consumers automáticamente después de los binds
                        e.ConfigureConsumers(context);
                    });

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
                    cfg.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter());                    
                    cfg.Publish<IEvent>(p => p.ExchangeType = ExchangeType.Topic);

                    cfg.PublishTopology.BrokerTopologyOptions = PublishBrokerTopologyOptions.MaintainHierarchy;
                    cfg.DeployPublishTopology = true;
                    cfg.OverrideDefaultBusEndpointQueueName(rabbitMqOptions.ExchangeName);

                    cfg.ConnectConsumeObserver(new ConsumeObserver());

                    //cfg.ConfigureEndpoints(context);
                });

            });

        return services;
    }
}

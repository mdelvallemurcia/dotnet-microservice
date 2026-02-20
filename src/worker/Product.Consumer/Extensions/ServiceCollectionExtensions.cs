using Api.Features.Shared;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
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
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, "/", h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ReceiveEndpoint("worker-productos-principal", e =>
                    {
                        e.SetQuorumQueue(); // ojo! posible procesamiento duplicado, pero garantiza alta disponibilidad y durabilidad
                        // 1. Prefetch Count: Cuántos mensajes se bajan a la RAM de la App a la vez.
                        // Para volumen alto, un valor entre 16 y 32 suele ser el punto dulce.
                        e.PrefetchCount = 32;
                        e.ConcurrentMessageLimit = 10;

                        // 2. Vinculación al Topic con Wildcard
                        e.Bind("mi-exchange-de-productos", s =>
                        {
                            s.RoutingKey = "dm.product.*";
                            s.ExchangeType = ExchangeType.Topic;
                        });

                        // 3. Registrar los consumidores en este endpoint
                        e.ConfigureConsumers(context);
                        //e.ConfigureConsumer<ProjectAddedConsumer>(context);
                    });

                    cfg.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter(rabbitMqOptions));
                    cfg.PublishTopology.BrokerTopologyOptions = PublishBrokerTopologyOptions.MaintainHierarchy;
                    cfg.DeployPublishTopology = true;

                    
                });

            });

        return services;
    }
}

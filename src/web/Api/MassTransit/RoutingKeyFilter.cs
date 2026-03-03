using MassTransit;
using Models.Events;

namespace api.MassTransit;

internal class RoutingKeyFilter : IFilter<PublishContext<IEvent>>
{
    public Task Send(PublishContext<IEvent> context, IPipe<PublishContext<IEvent>> next)
    {
        var routingKey = context.Message.GetType().FullName ?? string.Empty;
        if (!string.IsNullOrEmpty(routingKey))
        {
            // Acceder al contexto de RabbitMQ para establecer el routing key
            // En MassTransit, Publish internamente usa Send, por lo que usamos RabbitMqSendContext
            var sendContext = context.GetPayload<RabbitMqSendContext>();
            if (sendContext != null)
            {
                sendContext.RoutingKey = routingKey;
            }
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("routingKey");
}

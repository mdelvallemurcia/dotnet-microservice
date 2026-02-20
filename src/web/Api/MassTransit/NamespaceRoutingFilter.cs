using MassTransit;

namespace api.MassTransit;

public class NamespaceRoutingFilter<T> : IFilter<PublishContext<T>> where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var ns = typeof(T).Namespace;
        if (!string.IsNullOrEmpty(ns))
            //context.PublishByRoutingKey<IEvent>($"{ns}.{typeof(T).Name}");
            context.SetRoutingKey($"{ns}.{typeof(T).Name}");
        await next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("namespaceRouting");
}

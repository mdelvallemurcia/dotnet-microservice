using MassTransit;
using Models.Events;

namespace ProjectSubscriber.Consumers;

public class ProjectAddedConsumer : IConsumer<ProjectAdded>
{
    public async Task Consume(ConsumeContext<ProjectAdded> context)
    {
        var mensaje = context.Message;
        Console.WriteLine($"Procesando pago del pedido: {mensaje.EventId}");

        await Task.CompletedTask;
    }
}

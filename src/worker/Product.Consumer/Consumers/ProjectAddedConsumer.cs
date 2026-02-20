using MassTransit;
using Models.Events.Project;

namespace ProjectSubscriber.Consumers;

public class ProjectAddedConsumer : IConsumer<ProjectAdded>
{
    public async Task Consume(ConsumeContext<ProjectAdded> context)
    {
        var mensaje = context.Message;
        Console.WriteLine($"Project created event: {mensaje.EventId}");

        await Task.CompletedTask;
    }
}

using MassTransit;

namespace ProjectSubscriber.Consumers;

//public class ProjectRemovedConsumer : IConsumer<ProjectRemoved>
//{
//    public async Task Consume(ConsumeContext<ProjectRemoved> context)
//    {
//        var mensaje = context.Message;
//        Console.WriteLine($"Project removed event: {mensaje.EventId}");

//        await Task.CompletedTask;
//    }
//}

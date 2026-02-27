using MassTransit;
using Models.Events.Project;

namespace ProjectSubscriber.Consumers;

public class ProjectAddedConsumer : IConsumer<ProjectAdded>
{
    private readonly ILogger<ProjectAddedConsumer> _logger;
    public ProjectAddedConsumer(ILogger<ProjectAddedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProjectAdded> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation($"Project created event: {mensaje.EventId}");

        await Task.CompletedTask;
    }
}

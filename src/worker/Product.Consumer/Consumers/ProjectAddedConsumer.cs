using MassTransit;
using Models.Events.Project;

namespace ProjectSubscriber.Consumers;

internal class ProjectAddedConsumer : IConsumer<ProjectAdded>
{
    private readonly ILogger<ProjectAddedConsumer> _logger;
    public ProjectAddedConsumer(ILogger<ProjectAddedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProjectAdded> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation($"Project created event: {mensaje.EventId}");

        return Task.CompletedTask;
    }
}

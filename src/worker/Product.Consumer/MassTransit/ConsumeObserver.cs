using MassTransit;

namespace ProjectSubscriber.MassTransit;

public class ConsumeObserver : IConsumeObserver
{
    private readonly ILogger<ConsumeObserver> _logger;

    public ConsumeObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    // Runs as soon as the message arrives from RabbitMQ, before the consumer is resolved.
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogDebug(
            "Message received: {MessageType} (Id: {MessageId}, RoutingKey: {RoutingKey})",
            typeof(T).Name, context.MessageId, context.RoutingKey());

        return Task.CompletedTask;
    }

    // Runs after the consumer finishes successfully.
    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogDebug("Message processed successfully: {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    // Runs if the consumer throws.
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogError(exception, "Error processing {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}

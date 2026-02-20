namespace Api.Features.Services.EventPublisher;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message);
    //TODO Task Publish<IEnumerable<T>>(IEnumerable<T> messages);
}

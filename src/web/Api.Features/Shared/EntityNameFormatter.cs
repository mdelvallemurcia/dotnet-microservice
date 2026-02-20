using MassTransit;

namespace Api.Features.Shared;

public class EntityNameFormatter : IEntityNameFormatter
{
    private readonly string _invalidTopicQueue;
    private readonly Dictionary<string, string> _topicMapping;

    public EntityNameFormatter(RabbitMqOptions options)
    {
        _topicMapping = options.Topics;
        _invalidTopicQueue = options.InvalidTopicQueue;
    }

    public string FormatEntityName<T>()
    {
        var classFullName = typeof(T).FullName!;
        if (_topicMapping.TryGetValue(classFullName, out var topic))
            return topic; 

        return _invalidTopicQueue;
    }
}

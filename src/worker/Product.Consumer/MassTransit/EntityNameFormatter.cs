using MassTransit;

namespace ProjectSubscriber.MassTransit;

public class EntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).FullName ?? string.Empty;
    }
}

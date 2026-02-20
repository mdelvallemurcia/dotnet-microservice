using MassTransit;

namespace api.MassTransit;

public class EntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return (typeof(T).FullName ?? "default-exchange");
    }
}

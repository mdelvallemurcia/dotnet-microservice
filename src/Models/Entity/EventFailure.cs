using Models.Events;

namespace Models.Entity;

public record EventFailure<T>(T Event) : BaseEntity where T : IEvent
{
    public string Type { get; init; } = typeof(T).FullName??nameof(T);
    public DateTime CreationDateTime { get; init; } = DateTime.UtcNow;
}

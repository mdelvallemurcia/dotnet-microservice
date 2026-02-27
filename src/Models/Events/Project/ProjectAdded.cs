namespace Models.Events.Project;

public record ProjectAdded(
    Guid Id,
    DateTime CreationDate,
    string Name
) : IEvent
{
    public ProjectAdded() : this(Guid.CreateVersion7(), DateTime.UtcNow, string.Empty)
    {
        EventId = Guid.CreateVersion7();
    }

    public Guid EventId { get; init; } = Guid.CreateVersion7();
};

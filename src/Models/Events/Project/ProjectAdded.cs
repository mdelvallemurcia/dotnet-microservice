namespace Models.Events.Project;

public record ProjectAdded(
    Guid Id,
    DateTime CreationDate,
    string Name
) : IEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
};

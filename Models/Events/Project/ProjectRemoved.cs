namespace Models.Events.Project;

public record ProjectRemoved(
    Guid Id,
    DateTime RemovedDate
) : IEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
};

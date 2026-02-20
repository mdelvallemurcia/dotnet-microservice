namespace Models.Events;

public record ProjectAdded(
    Guid Id,
    DateTime CreationDate,
    string Name
)
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
};

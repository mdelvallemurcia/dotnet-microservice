namespace Models.Events;

public interface IEvent
{
    public Guid EventId { get; init; }
}

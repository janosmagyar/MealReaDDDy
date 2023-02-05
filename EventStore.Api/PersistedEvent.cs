namespace EventStore.Api;

public record PersistedEvent
{
    public required GlobalPosition GlobalPosition { get; init; }
    public required StreamPosition StreamPosition { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required StreamId StreamId { get; init; }
    public required Event Event { get; init; }
}
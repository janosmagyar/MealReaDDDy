namespace EventStore.Api;

public interface IEventStore
{
    public IEnumerable<PersistedEvent> Events(StreamId streamId);
    public IEnumerable<PersistedEvent> AllEvents();
    public void Save(StreamId streamId, IReadOnlyList<object> events);
    public void Save(StreamId streamId, StreamRevision revision, IReadOnlyList<object> events);
    public Task SubscribeAll(GlobalPosition from, Action<PersistedEvent> eventAppeared, CancellationToken token);
}

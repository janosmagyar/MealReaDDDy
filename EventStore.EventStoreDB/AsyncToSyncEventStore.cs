using EventStore.Api;

namespace EventStore.EventStoreDB;

public class AsyncToSyncEventStore : IEventStore
{
    private readonly IEventStoreAsync _eventStore;

    public AsyncToSyncEventStore(IEventStoreAsync eventStore)
    {
        _eventStore = eventStore;
    }

    public IEnumerable<PersistedEvent> Events(StreamId streamId)
        => _eventStore.EventsAsync(streamId).ToEnumerable();

    public IEnumerable<PersistedEvent> AllEvents()
        => _eventStore.AllEventsAsync().ToEnumerable();

    public void Save(StreamId streamId, IReadOnlyList<object> events)
        => _eventStore.SaveAsync(streamId, events).Wait();

    public void Save(StreamId streamId, Api.StreamRevision revision, IReadOnlyList<object> events)
    {
        try
        {
            _eventStore.SaveAsync(streamId, revision, events).Wait();
        }
        catch (AggregateException e)
        {
            throw e.InnerException;
        }
    }

    public Task SubscribeAll(GlobalPosition from, Action<PersistedEvent> eventAppeared, CancellationToken token)
        => _eventStore.SubscribeAll(from, eventAppeared, token);
}

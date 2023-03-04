namespace EventStore.Api;

public interface IEventStoreAsync
{
    IAsyncEnumerable<PersistedEvent> EventsAsync(StreamId streamId);
    IAsyncEnumerable<PersistedEvent> AllEventsAsync();
    Task SaveAsync(StreamId streamId, IReadOnlyList<object> events);
    Task SaveAsync(StreamId streamId, Api.StreamRevision revision, IReadOnlyList<object> events);
    Task SubscribeAll(GlobalPosition from, Action<PersistedEvent> eventAppeared, CancellationToken token);
}

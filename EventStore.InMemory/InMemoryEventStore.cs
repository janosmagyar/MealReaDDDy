using EventStore.Api;

namespace EventStore.InMemory;

public class InMemoryEventStore : IEventStore
{
    private readonly IClock _clock;
    private readonly List<PersistedEvent> _events = new();

    public InMemoryEventStore(IClock clock) => _clock = clock;

    public IEnumerable<PersistedEvent> Events(StreamId streamId)
        => _events.Where(e => e.StreamId == streamId);

    public IEnumerable<PersistedEvent> AllEvents() => _events;

    public void Save(
        StreamId streamId,
        IReadOnlyList<Event> events)
    {
        lock (_events) SaveInternal(streamId, events);
    }

    public void Save(
        StreamId streamId,
        StreamRevision revision,
        IReadOnlyList<Event> events)
    {
        lock (_events)
        {
            var currentRevision = _events.Any(e => e.StreamId == streamId)
                ? (StreamRevision)_events.Last(e => e.StreamId == streamId).StreamPosition
                : StreamRevision.NotExists;

            if (currentRevision == StreamRevision.NotExists && revision != StreamRevision.NotExists)
                throw new EventStoreException($"Stream not exists! ({streamId})");

            if (currentRevision != StreamRevision.NotExists && revision == StreamRevision.NotExists)
                throw new EventStoreException($"Stream already exists! ({streamId})");

            if (currentRevision != revision)
                throw new EventStoreException($"Stream modified! ({streamId}, current rev.: {currentRevision}, wanted rev.: {revision})");

            SaveInternal(streamId, events);
        }
    }

    private void SaveInternal(
        StreamId streamId,
        IEnumerable<Event> events)
    {
        foreach (var @event in events)
        {
            var e = new PersistedEvent()
            {
                StreamId = streamId,
                Event = @event with { },
                CreatedUtc = _clock.UtcNow,
                GlobalPosition = _events.LongCount(),
                StreamPosition = _events.LongCount(e => e.StreamId == streamId),
            };
            _events.Add(e);
        }
    }

    public Task SubscribeAll(
        GlobalPosition from,
        Action<PersistedEvent> eventAppeared,
        CancellationToken token)
        => Task.Run(() => new Subscription(_events).Subscribe(from, eventAppeared, token), token);
}
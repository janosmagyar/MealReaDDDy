using System.Text;
using EventStore.Api;
using EventStore.Client;
using Newtonsoft.Json;
using StreamPosition = EventStore.Client.StreamPosition;
using StreamRevision = EventStore.Client.StreamRevision;

namespace EventStore.EventStoreDB
{
    public class EventStoreDbAdapter : IEventStoreAsync
    {
        private readonly IClock _clock;
        private readonly EventStoreClient _client;

        public EventStoreDbAdapter(string connectionString, IClock clock)
        {
            _clock = clock;
            _client = new EventStoreClient(
                EventStoreClientSettings.Create(connectionString)
            );
        }

        IAsyncEnumerable<PersistedEvent> IEventStoreAsync.EventsAsync(StreamId streamId)
        {
            var result = _client.ReadStreamAsync(Direction.Forwards, streamId, StreamPosition.Start);
            return result.ReadState.Result == ReadState.StreamNotFound
                ? AsyncEnumerable.Empty<PersistedEvent>()
                : result
                    .SelectAwait(resolvedEvent => new ValueTask<PersistedEvent>(Convert(resolvedEvent)));
        }

        IAsyncEnumerable<PersistedEvent> IEventStoreAsync.AllEventsAsync() =>
            _client.ReadAllAsync(Direction.Forwards, Position.Start)
                .WhereAwait(e => new ValueTask<bool>(!e.Event.EventStreamId.StartsWith("$")))
                .SelectAwait(resolvedEvent => new ValueTask<PersistedEvent>(Convert(resolvedEvent)));

        public async Task SaveAsync(StreamId streamId, IReadOnlyList<object> events) =>
            await _client.AppendToStreamAsync(streamId, StreamState.Any, Convert(events));

        public async Task SaveAsync(StreamId streamId, Api.StreamRevision revision, IReadOnlyList<object> events)
        {
            try
            {
                if (revision == Api.StreamRevision.NotExists)
                    await _client.AppendToStreamAsync(streamId, StreamState.NoStream, Convert(events));
                else
                    await _client.AppendToStreamAsync(streamId, new StreamRevision(revision), Convert(events));
            }
            catch (WrongExpectedVersionException e)
            {
                if (e.ActualStreamRevision == StreamRevision.None)
                    throw new EventStoreException($"Stream not exists! ({streamId})");
                if (e.ActualStreamRevision != StreamRevision.None && revision == Api.StreamRevision.NotExists)
                    throw new EventStoreException($"Stream already exists! ({streamId})");
                throw new EventStoreException(
                    $"Stream modified! ({streamId}, current rev.: {e.ActualVersion}, wanted rev.: {revision})");
            }
        }

        public async Task SubscribeAll(
            GlobalPosition from,
            Action<PersistedEvent> eventAppeared,
            CancellationToken token)
        {
            await _client
                .SubscribeToAllAsync(
                    FromAll.After(new Position(from, from)),
                    (_, resolvedEvent, cancellationToken)
                        => Task.Run(() =>
                        {
                            if (resolvedEvent.Event.EventStreamId.StartsWith("$"))
                                return;
                            eventAppeared.Invoke(Convert(resolvedEvent));
                        }, cancellationToken),
                    cancellationToken: token);
        }

        private IEnumerable<EventData> Convert(IEnumerable<object> events)
        {
            foreach (var @event in events)
            {
                var type = @event.GetType();
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
                var meta = new Metadata() { CreatedUtc = _clock.UtcNow, TypeName = type.AssemblyQualifiedName! };
                var metadata =
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(meta));
                yield return new EventData(Uuid.NewUuid(), type.FullName!, data, metadata);
            }
        }

        private PersistedEvent Convert(ResolvedEvent resolvedEvent)
        {
            var metadata = JsonConvert.DeserializeObject<Metadata>(
                Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.ToArray()));

            var type = Type.GetType(metadata!.TypeName);
            var data = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data.ToArray()),
                type!);

            return new PersistedEvent()
            {
                StreamPosition = resolvedEvent.Event.EventNumber.ToUInt64(),
                GlobalPosition = resolvedEvent.Event.Position.CommitPosition,
                Event = data!,
                StreamId = resolvedEvent.Event.EventStreamId,
                CreatedUtc = metadata.CreatedUtc,
            };
        }

        private class Metadata
        {
            public DateTime CreatedUtc { get; init; }
            public string TypeName { get; init; } = null!;
        }
    }
}

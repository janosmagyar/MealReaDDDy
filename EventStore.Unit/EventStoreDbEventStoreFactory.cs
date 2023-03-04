using EventStore.Api;
using EventStore.EventStoreDB;

namespace EventStore.Unit;

public class EventStoreDbEventStoreFactory : IEventStoreFactory
{
    public IEventStore Create(IClock clock)
    {
        return new AsyncToSyncEventStore(
            new EventStoreDbAdapter("esdb://localhost:2113?tls=false",clock));
    }
}

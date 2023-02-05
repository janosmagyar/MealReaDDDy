using EventStore.Api;
using EventStore.InMemory;

namespace EventStore.Unit;

public class InMemoryEventStoreFactory : IEventStoreFactory
{
    public IEventStore Create(IClock clock)
    {
        return new InMemoryEventStore(clock);
    }
}
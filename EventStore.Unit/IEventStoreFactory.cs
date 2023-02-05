using EventStore.Api;

namespace EventStore.Unit;

public interface IEventStoreFactory
{
    IEventStore Create(IClock clock);
}
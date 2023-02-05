using EventStore.Api;

namespace EventStore.Unit;

public record TestEvent(string Comment) : Event;
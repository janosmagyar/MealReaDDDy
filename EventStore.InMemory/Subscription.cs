using EventStore.Api;

namespace EventStore.InMemory;

internal class Subscription
{
    private readonly IEnumerable<PersistedEvent> _events;
    public Subscription(IEnumerable<PersistedEvent> events)
    {
        _events = events;
    }

    public void Subscribe(
        GlobalPosition from,
        Action<PersistedEvent> eventAppeard,
        CancellationToken cancellationToken)
    {
        var position = from;

        while (true)
        {
            var e = _events.FirstOrDefault(g => g.GlobalPosition == position);
            if (e != null)
            {
                eventAppeard.Invoke(e);
                position = position.Next;
            }
            else
                Thread.Sleep(100);

            if (cancellationToken.IsCancellationRequested)
                return;
        }
    }
}
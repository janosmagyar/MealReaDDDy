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
            var e = _events.FirstOrDefault(g => g.GlobalPosition == position.Next);
            if (e != null)
            {
                eventAppeard.Invoke(e);
                position = position.Next;
            }
            else
                Task.Delay(50, cancellationToken).Wait(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;
        }
    }
}

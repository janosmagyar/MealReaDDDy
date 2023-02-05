namespace EventStore.Api;

public class EventStoreException : Exception
{
    public EventStoreException(string message):base(message)
    {

    }
}
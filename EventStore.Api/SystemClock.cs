namespace EventStore.Api;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
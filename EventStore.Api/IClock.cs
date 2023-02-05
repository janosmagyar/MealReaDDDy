namespace EventStore.Api;

public interface IClock
{
    public DateTime UtcNow { get; }
}
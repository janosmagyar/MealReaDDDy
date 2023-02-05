namespace EventStore.Api;

public readonly record struct StreamRevision : IComparable<StreamRevision>
{
    private readonly ulong _position = 0;

    public StreamRevision(ulong position)
    {
        _position = position;
    }

    public static implicit operator ulong(StreamRevision p) => p._position;
    public static implicit operator StreamRevision(ulong p) => new(p);

    public static implicit operator StreamPosition(StreamRevision p) => new(p._position);
    public static implicit operator StreamRevision(StreamPosition p) => new(p);

    public static StreamRevision NotExists => new(ulong.MaxValue);
    public int CompareTo(StreamRevision other)
    {
        return _position.CompareTo(other._position);
    }

    public override string ToString()
    {
        return _position.ToString();
    }
}
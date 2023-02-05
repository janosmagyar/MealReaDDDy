namespace EventStore.Api;

public readonly record struct StreamPosition : IComparable<StreamPosition>
{
    private readonly ulong _position = 0;

    public StreamPosition(ulong position)
    {
        if (position == ulong.MaxValue)
            throw new ArgumentOutOfRangeException();
        _position = position;
    }

    public static implicit operator ulong(StreamPosition p) => p._position;
    public static implicit operator StreamPosition(ulong p) => new(p);

    public static implicit operator long(StreamPosition p) => (long)p._position;
    public static implicit operator StreamPosition(long p) => new((ulong)p);

    public int CompareTo(StreamPosition other)
    {
        return _position.CompareTo(other._position);
    }

    public override string ToString()
    {
        return _position.ToString();
    }
}
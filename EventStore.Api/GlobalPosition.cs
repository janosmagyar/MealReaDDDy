namespace EventStore.Api;

public readonly record struct GlobalPosition : IComparable<GlobalPosition>
{
    private readonly ulong _position = 0;
    public static readonly GlobalPosition Start = new(0);
    public GlobalPosition(ulong position)
    {
        _position = position;
    }

    public static implicit operator ulong(GlobalPosition p) => p._position;
    public static implicit operator GlobalPosition(ulong p) => new(p);

    public static implicit operator long(GlobalPosition p) => (long)p._position;
    public static implicit operator GlobalPosition(long p) => new((ulong)p);

    public GlobalPosition Next => new(_position + 1UL);
    public int CompareTo(GlobalPosition other)
    {
        return _position.CompareTo(other._position);
    }

    public override string ToString()
    {
        return _position.ToString();
    }
}
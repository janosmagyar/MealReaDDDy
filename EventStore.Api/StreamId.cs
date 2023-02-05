namespace EventStore.Api;

public readonly record struct StreamId
{
    private readonly string _id = String.Empty;

    public StreamId(string id)
    {
        _id = id;
    }

    public StreamId()
    {
        _id = Guid.NewGuid().ToString("N");
    }

    public static implicit operator string(StreamId s) => s._id;
    public static implicit operator StreamId(string s) => new(s);
    public override string ToString()
    {
        return _id;
    }
}
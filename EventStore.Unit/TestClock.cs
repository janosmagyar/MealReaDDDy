using EventStore.Api;

namespace EventStore.Unit;

internal class TestClock : IClock
{
    private DateTime? _mNow;
    public DateTime UtcNow => _mNow ?? throw new InvalidOperationException("Clock not set!");
    public TestClock Set(DateTime time)
    {
        if (_mNow.HasValue && _mNow.Value > time) throw new ArgumentException("Can't set backwards!");
        _mNow = time;
        return this;
    }
}
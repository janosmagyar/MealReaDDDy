namespace Meal.Ordering.Api;

public record ItemIndex
{
    private readonly int _index;

    public ItemIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(message: "Invalid item index!", null);
        _index = index;
    }

    public static implicit operator int(ItemIndex i) => i._index;
    public static implicit operator ItemIndex(int i) => new(i);
}
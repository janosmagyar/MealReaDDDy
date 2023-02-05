namespace Meal.Ordering.Api;

public readonly record struct TableNumber
{
    public readonly int? Value = null;
    public bool IsEmpty => !Value.HasValue;
    public TableNumber(int? value)
    {
        if (value is < 0 or > 99)
            throw new ArgumentException("Invalid table number!", nameof(value));
        Value = value;
    }

    public override string ToString() => Value.HasValue ? $"{Value.Value:D2}" : "empty";

    public static implicit operator int?(TableNumber tn) => tn.Value;
    public static implicit operator TableNumber(int? n) =>new (n);
}

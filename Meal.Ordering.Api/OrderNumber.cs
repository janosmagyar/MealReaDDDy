namespace Meal.Ordering.Api;

public record  OrderNumber
{
    public readonly int Value;
    public OrderNumber(int value)
    {
        if (value is < 0 or > 999)
            throw new ArgumentException("Invalid order number!", nameof(value));
        Value = value;
    }

    public override string ToString() => $"{Value:D3}";

    public static implicit operator int(OrderNumber tn) => tn.Value;
    public static implicit operator OrderNumber(int n) => new(n);
}

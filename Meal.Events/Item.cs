namespace Meal.Events;

public record Item
{
    public required int Count { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
}

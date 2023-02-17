namespace Meal.Events;

public record MealOrdered
{
    public required int OrderNumber  { get; init; }
    public required int? Table { get; init; }
    public required string Serving { get; init; }
    public required Item[] Items { get; init; }
}

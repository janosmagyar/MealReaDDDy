namespace Meal.Events;

public record MealItemPrepared
{
    public required int ItemIndex { get; init; }
}

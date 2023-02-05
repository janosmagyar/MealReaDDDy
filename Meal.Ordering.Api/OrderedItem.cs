namespace Meal.Ordering.Api;

public record OrderedItem(int Count, string Name, Category Category, bool IsPrepared)
    : FoodItem(Count, Name, Category);

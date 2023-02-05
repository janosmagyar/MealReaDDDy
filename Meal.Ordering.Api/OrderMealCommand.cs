namespace Meal.Ordering.Api;

public record OrderMealCommand(FoodItem[] Items, Serving Serving, TableNumber Table);
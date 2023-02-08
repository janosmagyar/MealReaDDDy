namespace Meal.Ordering;

internal interface IEventHandler
{
    public MealProjectedState Apply(MealProjectedState state);
}
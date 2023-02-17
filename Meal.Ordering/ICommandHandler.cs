namespace Meal.Ordering;

internal interface ICommandHandler
{
    public IEnumerable<object> Events(MealProjectedState state);
}

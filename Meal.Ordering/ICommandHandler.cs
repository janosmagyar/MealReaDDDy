using EventStore.Api;

namespace Meal.Ordering;

internal interface ICommandHandler
{
    public IEnumerable<Event> Events(MealProjectedState state);
}
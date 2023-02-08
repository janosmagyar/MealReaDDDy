using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class PickedUpEventHandler : IEventHandler
{
    private readonly MealPickedUp _event;

    public PickedUpEventHandler(MealPickedUp @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.State = OrderState.Done;
        return state;
    }
}
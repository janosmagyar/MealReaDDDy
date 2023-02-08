using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class TakenAwayEventHandler : IEventHandler
{
    private readonly MealTakenAway _event;

    public TakenAwayEventHandler(MealTakenAway @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.State = OrderState.Done;
        return state;
    }
}
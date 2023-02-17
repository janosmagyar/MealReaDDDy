using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class ServedToTableEventHandler : IEventHandler
{
    private readonly MealServedToTable _event;

    public ServedToTableEventHandler(MealServedToTable @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.State = OrderState.Done;
        return state;
    }
}

using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class ServedTotTableEventHandler : IEventHandler
{
    private readonly MealServedToTable _event;

    public ServedTotTableEventHandler(MealServedToTable @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.State = OrderState.Done;
        return state;
    }
}
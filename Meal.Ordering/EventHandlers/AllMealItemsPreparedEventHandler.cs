using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class AllMealItemsPreparedEventHandler : IEventHandler
{
    private readonly AllMealItemsPrepared _event;
    public AllMealItemsPreparedEventHandler(AllMealItemsPrepared @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.State = OrderState.Ready;
        return state;
    }
}

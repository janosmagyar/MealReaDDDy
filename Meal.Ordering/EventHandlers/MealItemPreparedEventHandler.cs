using Meal.Events;

namespace Meal.Ordering.EventHandlers;

internal class MealItemPreparedEventHandler : IEventHandler
{
    private readonly MealItemPrepared _event;

    public MealItemPreparedEventHandler(MealItemPrepared @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.Items[_event.ItemIndex].PrepareOne();
        return state;
    }
}

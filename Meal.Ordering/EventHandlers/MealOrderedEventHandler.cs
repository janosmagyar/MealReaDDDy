using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class MealOrderedEventHandler : IEventHandler
{
    private readonly MealOrdered _event;

    public MealOrderedEventHandler(MealOrdered @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        return new MealProjectedState()
        {
            OrderNumber = _event.OrderNumber,
            Table = _event.Table,
            Items = _event.Items
                .Select(i => new TrackedItem(i.Count, i.Name, Enum.Parse<Category>(i.Category)))
                .ToArray(),
            Serving = Enum.Parse<Serving>(_event.Serving),
            State = OrderState.InPreparation,
            Payment = PaymentState.Waiting,
        };
    }
}
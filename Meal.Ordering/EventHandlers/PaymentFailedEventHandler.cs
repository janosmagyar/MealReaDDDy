using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class PaymentFailedEventHandler : IEventHandler
{
    private readonly PaymentFailed _event;

    public PaymentFailedEventHandler(PaymentFailed @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.Payment = PaymentState.Failed;
        return state;
    }
}

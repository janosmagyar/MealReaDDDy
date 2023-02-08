using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.EventHandlers;

internal class PaymentSucceededEventHandler : IEventHandler
{
    private readonly PaymentSucceeded _event;

    public PaymentSucceededEventHandler(PaymentSucceeded @event)
    {
        _event = @event;
    }

    public MealProjectedState Apply(MealProjectedState state)
    {
        state.Payment = PaymentState.Successful;
        return state;
    }
}
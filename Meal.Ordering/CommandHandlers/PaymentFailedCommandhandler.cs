using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

public class PaymentFailedCommandhandler : ICommandHandler
{
    public IEnumerable<Event> Events(MealProjectedState state)
    {
        if (state.Payment == PaymentState.Successful)
            throw new InvalidOperationException("Payment was already successful!");
        yield return new PaymentFailed();
    }
}
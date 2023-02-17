using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

public class PaymentSucceededCommandhandler : ICommandHandler
{
    public IEnumerable<object> Events(MealProjectedState state)
    {
        if (state.Payment == PaymentState.Successful)
            throw new InvalidOperationException("Payment was already successful!");
        yield return new PaymentSucceeded();
    }
}

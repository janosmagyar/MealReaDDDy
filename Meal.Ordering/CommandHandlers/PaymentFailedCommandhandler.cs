using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

internal class PaymentFailedCommandhandler : ICommandHandler
{
    public IEnumerable<object> Events(MealProjectedState state)
    {
        if (state.Payment == PaymentState.Successful)
            throw new InvalidOperationException("Payment was already successful!");
        yield return new PaymentFailed();
    }
}

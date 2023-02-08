using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

public class TakeAwayMealCommandhandler : ICommandHandler
{
    public IEnumerable<Event> Events(MealProjectedState state)
    {
        if (state.Payment != PaymentState.Successful)
            throw new InvalidOperationException("Payment wasn't successful!");
        if (state.Serving != Serving.paperbag)
            throw new InvalidOperationException("Wrong serving!");
        yield return new MealTakenAway();
    }
}
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

internal class ServeMealToTableCommandhandler : ICommandHandler
{
    public IEnumerable<object> Events(MealProjectedState state)
    {
        if (state.Payment != PaymentState.Successful)
            throw new InvalidOperationException("Payment wasn't successful!");
        if (state.Serving != Serving.tray || state.Table.IsEmpty)
            throw new InvalidOperationException("Wrong serving!");
        yield return new MealServedToTable();
    }
}

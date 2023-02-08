using Meal.Ordering.Api;
using Meal.Ordering.CommandHandlers;

namespace Meal.Ordering;

internal static class MealCommandHandlers
{
    private static readonly IDictionary<OrderState, ISet<Type>> AllowedCommandHandlersForStates = new Dictionary<OrderState, ISet<Type>>
    {
        {
            OrderState.None,
            new HashSet<Type>
            {
                typeof(OrderMealCommandhandler),
            }
        },
        {
            OrderState.InPreparation,
            new HashSet<Type>
            {
                typeof(ConfirmMealItemPreparedCommandhandler),
                typeof(PaymentSucceededCommandhandler),
                typeof(PaymentFailedCommandhandler),
            }
        },
        {
            OrderState.Ready,
            new HashSet<Type>
            {
                typeof(PaymentFailedCommandhandler),
                typeof(PaymentSucceededCommandhandler),
                typeof(TakeAwayMealCommandhandler),
                typeof(PickUpMealCommandhandler),
                typeof(ServeMealToTableCommandhandler),
            }
        }
    };

    public static void CheckAllowedInState(Type type, OrderState state)
    {
        if (!AllowedCommandHandlersForStates.ContainsKey(state))
            throw new InvalidOperationException($"Unknown state!");
        if (!AllowedCommandHandlersForStates[state].Contains(type))
            throw new InvalidOperationException("Invalid command for state!");
    }
}

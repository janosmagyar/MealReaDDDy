using Meal.Events;
using Meal.Ordering.EventHandlers;

namespace Meal.Ordering;

internal static class MealEventHandlers
{
    private static readonly Dictionary<Type, Func<object, IEventHandler>> EventHandlers = new()
    {
        {typeof(MealOrdered), (e)=> new MealOrderedEventHandler((e as MealOrdered)!)},
        {typeof(MealItemPrepared), (e)=> new MealItemPreparedEventHandler((e as MealItemPrepared)!)},
        {typeof(AllMealItemsPrepared), (e)=> new AllMealItemsPreparedEventHandler((e as AllMealItemsPrepared)!)},
        {typeof(PaymentFailed), (e)=> new PaymentFailedEventHandler((e as PaymentFailed)!)},
        {typeof(PaymentSucceeded), (e)=> new PaymentSucceededEventHandler((e as PaymentSucceeded)!)},
        {typeof(MealTakenAway), (e)=> new TakenAwayEventHandler((e as MealTakenAway)!)},
        {typeof(MealPickedUp), (e)=> new PickedUpEventHandler((e as MealPickedUp)!)},
        {typeof(MealServedToTable), (e)=> new ServedToTableEventHandler((e as MealServedToTable)!)},
    };

    public static IEventHandler Create(object @event)
    {
        if (EventHandlers.ContainsKey(@event.GetType()))
            return EventHandlers[@event.GetType()].Invoke(@event);
        throw new ArgumentException($"Unknow event type! ({@event.GetType().FullName})");
    }
}

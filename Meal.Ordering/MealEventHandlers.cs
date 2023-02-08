using EventStore.Api;
using Meal.Events;
using Meal.Ordering.EventHandlers;

namespace Meal.Ordering;

internal static class MealEventHandlers
{
    private static readonly Dictionary<Type, Func<Event, IEventHandler>> EventHandlers = new()
    {
        {typeof(MealOrdered), (e)=> new MealOrderedEventHandler((e as MealOrdered)!)},
        {typeof(MealItemPrepared), (e)=> new MealItemPreparedEventHandler((e as MealItemPrepared)!)},
        {typeof(AllMealItemsPrepared), (e)=> new AllMealItemsPreparedEventHandler((e as AllMealItemsPrepared)!)},
        {typeof(PaymentFailed), (e)=> new PaymentFailedEventHandler((e as PaymentFailed)!)},
        {typeof(PaymentSucceeded), (e)=> new PaymentSucceededEventHandler((e as PaymentSucceeded)!)},
        {typeof(MealTakenAway), (e)=> new TakenAwayEventHandler((e as MealTakenAway)!)},
        {typeof(MealPickedUp), (e)=> new PickedUpEventHandler((e as MealPickedUp)!)},
        {typeof(MealServedToTable), (e)=> new ServedTotTableEventHandler((e as MealServedToTable)!)},
    };

    public static IEventHandler Create(Event e)
    {
        if (EventHandlers.ContainsKey(e.GetType()))
            return EventHandlers[e.GetType()].Invoke(e);
        throw new ArgumentException($"Unknow event type! ({e.GetType().FullName})");
    }
}

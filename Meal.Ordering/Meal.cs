using EventStore.Api;
using Meal.Ordering.Api;

namespace Meal.Ordering;

public class Meal
{
    private readonly IEventStore _eventStore;
    private readonly string _id;
    public MealPublicState MealPublicState => new();

    public Meal(IEventStore eventStore, string id)
    {
        _eventStore = eventStore;
        _id = id;
    }

    public void Order(OrderMealCommand command, IOrderNumberGenerator orderNumberGenerator) { }

    public void ConfirmItemPrepared(ConfirmMealItemPreparedCommand command){}

    public void PaymentFailed() { }

    public void PaymentSucceeded() { }

    public void TakeAway() { }

    public void PickUp() { }

    public void ServeToTable() { }
}

using EventStore.Api;
using Meal.Ordering.Api;
using Meal.Ordering.CommandHandlers;

namespace Meal.Ordering;


public class Meal
{
    private readonly IEventStore _eventStore;
    private readonly string _id;
    private MealProjectedState _mealProjectedState;
    public MealPublicState MealPublicState => new()
    {
        State = _mealProjectedState.State,
        Payment = _mealProjectedState.Payment,
        OrderNumber = _mealProjectedState.OrderNumber,
        Serving = _mealProjectedState.Serving,
        Table = _mealProjectedState.Table,
        Items = _mealProjectedState.Items
            .Select(i => new OrderedItem(
                i.Count,
                i.Name,
                i.Category,
                i.IsPrepared))
            .ToArray(),
    };

    public Meal(IEventStore eventStore, string id) : this(eventStore, id, new())
    {
    }

    public Meal(IEventStore eventStore, string id, MealProjectedState state)
    {
        _mealProjectedState = state;
        _eventStore = eventStore;
        _id = id;
        foreach (var persistedEvent in eventStore.Events(id))
            Apply(persistedEvent.Event);
    }

    private void Apply(Event @event) =>
        _mealProjectedState = MealEventHandlers.Create(@event).Apply(_mealProjectedState);

    public void Order(OrderMealCommand command, IOrderNumberGenerator orderNumberGenerator)
        => ApplyAndSave(new OrderMealCommandhandler(command, orderNumberGenerator).Events(_mealProjectedState));

    public void ConfirmItemPrepared(ConfirmMealItemPreparedCommand command)
        => ApplyAndSave(new ConfirmMealItemPreparedCommandhandler(command).Events(_mealProjectedState));

    public void PaymentFailed()
        => ApplyAndSave(new PaymentFailedCommandhandler().Events(_mealProjectedState));

    public void PaymentSucceeded()
        => ApplyAndSave(new PaymentSucceededCommandhandler().Events(_mealProjectedState));

    public void TakeAway()
        => ApplyAndSave(new TakeAwayMealCommandhandler().Events(_mealProjectedState));

    public void PickUp()
        => ApplyAndSave(new PickUpMealCommandhandler().Events(_mealProjectedState));

    public void ServeToTable()
        => ApplyAndSave(new ServeMealToTableCommandhandler().Events(_mealProjectedState));

    private void ApplyAndSave(IEnumerable<Event> events)
    {
        var eventList = events.ToList();
        eventList.ForEach(Apply);
        _eventStore.Save(_id, eventList);
    }
}

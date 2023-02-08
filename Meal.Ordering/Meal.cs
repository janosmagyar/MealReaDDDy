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
        => ApplyAndSave(new OrderMealCommandhandler(command, orderNumberGenerator));

    public void ConfirmItemPrepared(ConfirmMealItemPreparedCommand command)
        => ApplyAndSave(new ConfirmMealItemPreparedCommandhandler(command));

    public void PaymentFailed()
        => ApplyAndSave(new PaymentFailedCommandhandler());

    public void PaymentSucceeded()
        => ApplyAndSave(new PaymentSucceededCommandhandler());

    public void TakeAway()
        => ApplyAndSave(new TakeAwayMealCommandhandler());

    public void PickUp()
        => ApplyAndSave(new PickUpMealCommandhandler());

    public void ServeToTable()
        => ApplyAndSave(new ServeMealToTableCommandhandler());

    private void ApplyAndSave(ICommandHandler handler)
    {
        MealCommandHandlers.CheckAllowedInState(handler.GetType(), _mealProjectedState.State);
        var eventList = handler.Events(_mealProjectedState).ToList();
        eventList.ForEach(Apply);
        _eventStore.Save(_id, eventList);
    }
}

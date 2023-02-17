using Meal.Ordering.Api;
using Meal.Ordering.CommandHandlers;

namespace Meal.Ordering;

public class Meal
{
    private readonly List<object> _newEvents = new();
    private MealProjectedState _mealProjectedState;

    public ulong Revision { get; init; }
    public string Id { get; init; }
    public IReadOnlyList<object> NewEvents => _newEvents;

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

    public Meal(string id, IEnumerable<object> events, MealProjectedState? state, ulong revision)
    {
        _mealProjectedState = state ?? new MealProjectedState();
        Id = id;
        Revision = revision;
        foreach (var @event in events ?? Array.Empty<object>())
            Apply(@event);
    }

    private void Apply(object @event) =>
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
        _newEvents.AddRange(eventList);
    }
}

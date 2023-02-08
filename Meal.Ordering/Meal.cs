using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

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
    {
        ApplyAndSave(new OrderMealCommandhandler(command, orderNumberGenerator).Events(_mealProjectedState));
    }

    public void ConfirmItemPrepared(ConfirmMealItemPreparedCommand command)
    {
        if (_mealProjectedState.State != OrderState.InPreparation)
            throw new InvalidOperationException("Invalid command for state!");

        if (_mealProjectedState.Items.Length <= command.ItemIndex)
            throw new IndexOutOfRangeException("Invalid item index!");

        var item = _mealProjectedState.Items[command.ItemIndex];

        if (item.IsPrepared)
            throw new InvalidOperationException("This item is already prepared!");

        _eventStore.Save(_id, new[]
        {
            new MealItemPrepared
            {
                ItemIndex = command.ItemIndex
            }
        });

        var allOtherItems = _mealProjectedState.Items.Where((item, i) => i != command.ItemIndex);

        if (allOtherItems.All(i => i.IsPrepared)
            && item.IsOneMissing)
            _eventStore.Save(_id, new[]
            {
                new AllMealItemsPrepared()
            });
    }

    public void PaymentFailed()
    {
        if (_mealProjectedState.Payment == PaymentState.Successful)
            throw new InvalidOperationException("Payment was already successful!");
        _eventStore.Save(_id, new[]
        {
            new PaymentFailed()
        });
    }

    public void PaymentSucceeded()
    {
        if (_mealProjectedState.Payment == PaymentState.Successful)
            throw new InvalidOperationException("Payment was already successful!");
        _eventStore.Save(_id, new[]
        {
            new PaymentSucceeded()
        });
    }

    public void TakeAway()
    {
        if (_mealProjectedState.Payment != PaymentState.Successful)
            throw new InvalidOperationException("Payment wasn't successful!");
        if (_mealProjectedState.Serving != Serving.paperbag)
            throw new InvalidOperationException("Wrong serving!");
        _eventStore.Save(_id, new[]
        {
            new MealTakenAway()
        });
    }

    public void PickUp()
    {
        if (_mealProjectedState.Payment != PaymentState.Successful)
            throw new InvalidOperationException("Payment wasn't successful!");
        if (_mealProjectedState.Serving != Serving.tray || !_mealProjectedState.Table.IsEmpty)
            throw new InvalidOperationException("Wrong serving!");
        _eventStore.Save(_id, new[]
        {
            new MealPickedUp()
        });
    }

    public void ServeToTable()
    {
        if (_mealProjectedState.Payment != PaymentState.Successful)
            throw new InvalidOperationException("Payment wasn't successful!");
        if (_mealProjectedState.Serving != Serving.tray || _mealProjectedState.Table.IsEmpty)
            throw new InvalidOperationException("Wrong serving!");
        _eventStore.Save(_id, new[]
        {
            new MealServedToTable()
        });
    }

    private void ApplyAndSave(IEnumerable<Event> events)
    {
        var eventList = events.ToList();
        eventList.ForEach(Apply);
        _eventStore.Save(_id, eventList);
    }
}
internal interface ICommandHandler
{
    public IEnumerable<Event> Events(MealProjectedState state);
}

internal class OrderMealCommandhandler : ICommandHandler
{
    private readonly OrderMealCommand _command;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public OrderMealCommandhandler(OrderMealCommand command, IOrderNumberGenerator orderNumberGenerator)
    {
        _command = command;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public IEnumerable<Event> Events(MealProjectedState state)
    {
        if (state.State != OrderState.None)
            throw new InvalidOperationException("Invalid command for state!");

        if (_command.Serving == Serving.paperbag && !_command.Table.IsEmpty)
            throw new InvalidOperationException("Invalid serving!");

        yield return new MealOrdered
        {
            Items = _command.Items.Select(i => new Item()
            {
                Category = i.Category.ToString(),
                Count = i.Count,
                Name = i.Name
            }).ToArray(),
            OrderNumber = _orderNumberGenerator.GetNext(),
            Serving = _command.Serving.ToString(),
            Table = _command.Table,
        };
    }
}

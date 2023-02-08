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
        {
            switch (persistedEvent.Event)
            {
                case MealOrdered mealOrdered:
                    _mealProjectedState.OrderNumber = mealOrdered.OrderNumber;
                    _mealProjectedState.Table = mealOrdered.Table;
                    _mealProjectedState.Serving = Enum.Parse<Serving>(mealOrdered.Serving);
                    _mealProjectedState.Items = mealOrdered.Items
                        .Select(i => new TrackedItem(
                            i.Count,
                            i.Name,
                            Enum.Parse<Category>(i.Category)))
                        .ToArray();

                    _mealProjectedState.State = OrderState.InPreparation;
                    _mealProjectedState.Payment = PaymentState.Waiting;
                    break;
                case MealItemPrepared mealItemPrepared:
                    _mealProjectedState.Items[mealItemPrepared.ItemIndex].PrepareOne();
                    break;
                case AllMealItemsPrepared allMealItemsPrepared:
                    _mealProjectedState.State = OrderState.Ready;
                    break;
                case PaymentFailed paymentFailed:
                    _mealProjectedState.Payment = PaymentState.Failed;
                    break;
                case PaymentSucceeded paymentSucceeded:
                    _mealProjectedState.Payment = PaymentState.Successful;
                    break;
                case MealPickedUp mealPickedUp:
                    _mealProjectedState.State = OrderState.Done;
                    break;
                case MealServedToTable mealServedToTable:
                    _mealProjectedState.State = OrderState.Done;
                    break;
                case MealTakenAway mealTakenAway:
                    _mealProjectedState.State = OrderState.Done;
                    break;
            }
        }
    }

    public void Order(OrderMealCommand command, IOrderNumberGenerator orderNumberGenerator)
    {
        if (_mealProjectedState.State != OrderState.None)
            throw new InvalidOperationException("Invalid command for state!");

        if (command.Serving == Serving.paperbag && !command.Table.IsEmpty)
            throw new InvalidOperationException("Invalid serving!");

        _eventStore.Save(_id, new[]
        {
            new MealOrdered
            {
                Table = command.Table,
                Serving = command.Serving.ToString(),
                OrderNumber = orderNumberGenerator.GetNext(),
                Items = command.Items.Select(i => new Item
                {
                    Category = i.Category.ToString(),
                    Count = i.Count,
                    Name = i.Name
                }).ToArray()
            }
        });
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
}

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

    public void ConfirmItemPrepared(ConfirmMealItemPreparedCommand command){}

    public void PaymentFailed() { }

    public void PaymentSucceeded() { }

    public void TakeAway() { }

    public void PickUp() { }

    public void ServeToTable() { }
}

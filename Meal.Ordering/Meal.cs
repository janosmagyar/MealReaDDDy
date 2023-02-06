using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering;

public class Meal
{
    private readonly IEventStore _eventStore;
    private readonly string _id;
    private readonly OrderNumber _orderNumber;
    private readonly TableNumber _tableNumber;
    private readonly Serving _serving;
    private readonly TrackedItem[] _items;
    private readonly OrderState _state;
    private readonly PaymentState _paymentState;

    public MealPublicState MealPublicState => new()
    {
        OrderNumber = _orderNumber,
        Payment = _paymentState,
        Serving = _serving,
        Table = _tableNumber,
        State = _state,
        Items = _items
            .Select(i => new OrderedItem(i.Count, i.Name, i.Category, i.IsPrepared))
            .ToArray()
    };

    public Meal(IEventStore eventStore, string id)
    {
        _eventStore = eventStore;
        _id = id;
        foreach (var persistedEvent in eventStore.Events(id))
        {
            switch (persistedEvent.Event)
            {
                case MealOrdered mealOrdered:
                    _orderNumber = mealOrdered.OrderNumber;
                    _tableNumber = mealOrdered.Table;
                    _serving = Enum.Parse<Serving>(mealOrdered.Serving);
                    _items = mealOrdered.Items
                        .Select(i => new TrackedItem(
                            i.Count,
                            i.Name,
                            Enum.Parse<Category>(i.Category)))
                        .ToArray();

                    _state = OrderState.InPreparation;
                    _paymentState = PaymentState.Waiting;
                    break;
            }
        }
    }

    public void Order(OrderMealCommand command, IOrderNumberGenerator orderNumberGenerator)
    {
        if (_state != OrderState.None)
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

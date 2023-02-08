using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

public class OrderMealCommandhandler : ICommandHandler
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
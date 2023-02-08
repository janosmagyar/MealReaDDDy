using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;

namespace Meal.Ordering.CommandHandlers;

public class ConfirmMealItemPreparedCommandhandler : ICommandHandler
{
    private readonly ConfirmMealItemPreparedCommand _command;
    public ConfirmMealItemPreparedCommandhandler(ConfirmMealItemPreparedCommand command)
    {
        _command = command;
    }

    public IEnumerable<Event> Events(MealProjectedState state)
    {
        if (state.State != OrderState.InPreparation)
            throw new InvalidOperationException("Invalid command for state!");

        if (state.Items.Length <= _command.ItemIndex)
            throw new IndexOutOfRangeException("Invalid item index!");

        var item = state.Items[_command.ItemIndex];

        if (item.IsPrepared)
            throw new InvalidOperationException("This item is already prepared!");

        yield return new MealItemPrepared()
        {
            ItemIndex = _command.ItemIndex,
        };

        var allOtherItems = state.Items.Where((item, i) => i != _command.ItemIndex);

        if (allOtherItems.All(i => i.IsPrepared)
            && item.IsOneMissing)
            yield return new AllMealItemsPrepared();
    }
}

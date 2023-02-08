using EventStore.Api;
using Meal.Events;

namespace Compensation.View;

public class MealsNotReadyInTwoMinutesSubscription
{
    private readonly IMealReaDDDyRepository _repository;

    public MealsNotReadyInTwoMinutesSubscription(IMealReaDDDyRepository repository)
    {
        _repository = repository;
    }

    public GlobalPosition Position => _repository.GetPosition();

    public void Consume(PersistedEvent e)
    {
        switch (e.Event)
        {
            case MealOrdered:
                _repository.Add(new MealReaDDDy { Id = e.StreamId, Start = e.CreatedUtc });
                break;

            case AllMealItemsPrepared:
                var mealReady = _repository.Get(e.StreamId);
                mealReady.End = e.CreatedUtc;
                if (mealReady.Duration < new TimeSpan(0, 0, 2, 0))
                    _repository.Remove(mealReady.Id);
                else
                    _repository.Update(mealReady);
                break;
        }
        _repository.SetPosition(e.GlobalPosition);
    }
}

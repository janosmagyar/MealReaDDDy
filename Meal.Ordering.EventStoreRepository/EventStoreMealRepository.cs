using EventStore.Api;

namespace Meal.Ordering.EventStoreRepository;

public class EventStoreMealRepository
{
    private readonly IEventStore _eventStore;

    public EventStoreMealRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public Meal GetOrCreate(string id) => GetOrCreate(id, new MealProjectedState());
    public Meal GetOrCreate(string id, MealProjectedState startState)
    {
        var events = _eventStore.Events(id).ToArray();
        return new Meal(
            id,
            events.Select(e => e.Event),
            startState,
            events.Any()
                ? events[^1].StreamPosition
                : StreamRevision.NotExists);
    }

    public void Update(Meal meal)
    {
        if (meal.NewEvents.Any())
            _eventStore.Save(meal.Id, meal.Revision, meal.NewEvents);
    }
}

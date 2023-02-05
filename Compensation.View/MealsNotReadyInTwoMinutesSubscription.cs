using EventStore.Api;

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
        _repository.SetPosition(e.GlobalPosition);
    }
}

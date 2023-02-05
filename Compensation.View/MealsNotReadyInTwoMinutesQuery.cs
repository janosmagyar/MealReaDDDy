namespace Compensation.View;

public class MealsNotReadyInTwoMinutesQuery
{
    private readonly IMealReaDDDyRepository _repository;

    public MealsNotReadyInTwoMinutesQuery(IMealReaDDDyRepository repository)
    {
        _repository = repository;
    }

    public bool IsEligibleForCompensation(string id)
    {
        return _repository.Contains(id)
               && _repository.Get(id).IsReaDDDy;
    }
}
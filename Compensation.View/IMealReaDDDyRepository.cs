using EventStore.Api;

namespace Compensation.View;

public interface IMealReaDDDyRepository
{
    void Add(MealReaDDDy entity);
    bool Contains(string id);
    MealReaDDDy Get(string id);
    void Remove(string id);
    void SetPosition(GlobalPosition position);
    GlobalPosition GetPosition();
    void Update(MealReaDDDy entity);
}

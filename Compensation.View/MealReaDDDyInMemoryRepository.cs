using System.Collections.Concurrent;
using EventStore.Api;

namespace Compensation.View;

public class MealReaDDDyInMemoryRepository : IMealReaDDDyRepository
{
    private readonly IDictionary<string, MealReaDDDy> _mealIndex = new ConcurrentDictionary<string, MealReaDDDy>();
    private GlobalPosition _position = GlobalPosition.Start;

    public void Add(MealReaDDDy entity) => _mealIndex.Add(entity.Id, entity);

    public bool Contains(string id) => _mealIndex.ContainsKey(id);

    public MealReaDDDy Get(string id) => _mealIndex[id];

    public void Remove(string id) => _mealIndex.Remove(id);

    public void SetPosition(GlobalPosition position) => _position = position;

    public GlobalPosition GetPosition() => _position;

    public void Update(MealReaDDDy entity) {
        //in memory reference type NOP
    }
}

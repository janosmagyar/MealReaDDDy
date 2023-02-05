using BoDi;
using EventStore.Api;
using EventStore.InMemory;
using Meal.Ordering.Api;
using TechTalk.SpecFlow;

namespace Meal.Ordering.Specs.Support;

[Binding]
public class Infrastructure
{
    private readonly IObjectContainer _objectContainer;

    public Infrastructure(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    [BeforeScenario]
    public void Setup()
    {
        _objectContainer.RegisterTypeAs<SystemClock, IClock>();
        _objectContainer.RegisterTypeAs<InMemoryEventStore, IEventStore>();
        _objectContainer.RegisterFactoryAs<IOrderNumberGenerator>(
            (a) => new FixedOrderNumberGenerator(1));
    }
}
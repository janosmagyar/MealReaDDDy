using BoDi;
using Compensation.View;
using EventStore.Api;
using EventStore.InMemory;
using TechTalk.SpecFlow;

namespace Compenstation.View.Specs.Support;

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
        var testClock = new TestClock();
        _objectContainer.RegisterInstanceAs<IClock>(testClock);
        _objectContainer.RegisterInstanceAs(testClock);
        _objectContainer.RegisterInstanceAs<IMealReaDDDyRepository>(new MealReaDDDyInMemoryRepository());
        _objectContainer.RegisterInstanceAs<IEventStore>(new InMemoryEventStore(testClock));
    }
}
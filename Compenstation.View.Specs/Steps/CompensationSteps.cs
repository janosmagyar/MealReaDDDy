using System.Globalization;
using Compensation.View;
using Compenstation.View.Specs.Support;
using EventStore.Api;
using Meal.Events;
using Meal.Ordering.Api;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Compenstation.View.Specs.Steps;

[Binding]
public class CompensationSteps
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TestClock _clock;
    private readonly IEventStore _eventStore;
    private readonly MealsNotReadyInTwoMinutesQuery _query;

    public CompensationSteps(
        TestClock clock,
        IEventStore eventStore,
        MealsNotReadyInTwoMinutesQuery query,
        MealsNotReadyInTwoMinutesSubscription subscription
    )
    {
        _clock = clock;
        _eventStore = eventStore;
        _query = query;
        _eventStore
            .SubscribeAll(
                subscription.Position,
                subscription.Consume,
                _cancellationTokenSource.Token);
    }

    [AfterScenario]
    public void CancelSubscription()
    {
        _cancellationTokenSource.Cancel();
    }

    [Given(@"the time in UTC: '([^']*)'")]
    public void GivenTheTimeInUtc(DateTime time)
    {
        _clock.Set(time);
    }

    [Given(@"a meal ordered with id '([^']*)'")]
    public void GivenAMealOrderedToTakeAwayWithId(string id)
    {
        _eventStore.Save(id, new Event[]
        {
            new MealOrdered
            {
                OrderNumber = 12,
                Serving = Serving.paperbag.ToString(),
                Table = null,
                Items = new[]
                {
                    new Item
                    {
                        Category = Category.burger.ToString(),
                        Count = 1,
                        Name = "hamburger"
                    }
                }
            }
        });
    }

    [Given(@"the times goes by '([^']*)' seconds")]
    public void GivenTheTimesGoesBySeconds(int seconds)
    {
        _clock.Set(_clock.UtcNow.AddSeconds(seconds));
    }

    [Given(@"a meal is ready with id '([^']*)'")]
    public void GivenAMealIsReadyWithId(string id)
    {
        _eventStore.Save(id, new Event[]
        {
            new AllMealItemsPrepared()
        });
    }

    [When(@"processing events")]
    public void WhenProcessingEvents()
    {
        Thread.Sleep(200);
    }

    [Then(@"order '([^']*)' is not eligible for compensation")]
    public void ThenOrderIsNotEligibleForCompensation(string id)
    {
        Assert.False(_query.IsEligibleForCompensation(id));
    }

    [Then(@"order '([^']*)' is eligible for compensation")]
    public void ThenOrderIsEligibleForCompensation(string id)
    {
        Assert.True(_query.IsEligibleForCompensation(id));
    }

    [StepArgumentTransformation]
    private DateTime DateTimeUtc(string p)
    {
        return DateTime.ParseExact(p, "yyyy.MM.dd hh:mm:ss", null, DateTimeStyles.AssumeUniversal)
            .ToUniversalTime();
    }
}

using DeepEqual.Syntax;
using NUnit.Framework;
using EventStore.Api;
using EventStore.Client;
using StreamPosition = EventStore.Api.StreamPosition;
using StreamRevision = EventStore.Api.StreamRevision;

namespace EventStore.Unit;

[TestFixture(typeof(InMemoryEventStoreFactory))]
[TestFixture(typeof(EventStoreDbEventStoreFactory))]
public class EventStoreRequirements<TEventStoreFactory> where TEventStoreFactory : IEventStoreFactory, new()
{
    private readonly IClock _clock = new TestClock().Set(new DateTime(2022, 11, 28, 16, 14, 59, DateTimeKind.Utc));

    private IEventStore EventStore() => new TEventStoreFactory().Create(_clock);

    private static TestEvent TestEvent()
    {
        return new TestEvent("the brown fox jumps over lazy dog");
    }

    [Test]
    public void SavingNoEvents()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();

        Assert.DoesNotThrow(() => eventStore.Save(streamId, Array.Empty<object>()));
        Assert.That(eventStore.Events(streamId), Is.Empty);
    }

    [Test]
    public void SavingNoEvents_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();

        Assert.DoesNotThrow(() => eventStore.Save(streamId, StreamRevision.NotExists, Array.Empty<object>()));
        Assert.That(eventStore.Events(streamId), Is.Empty);
    }

    [Test]
    public void SavingAnEventAndReadingItBack()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        eventStore.Save(streamId, new object[] { e });

        var events = eventStore.Events(streamId);

        events.Single().Event.ShouldDeepEqual(e);
    }

    [Test]
    public void SavingMoreEventsAndReadingThemBack()
    {
        var eventStore = EventStore();
        var e1 = TestEvent();
        var e2 = new TestEvent("god yzal revo spmuj xof nworb eht");
        var streamId = new StreamId();

        eventStore.Save(streamId, new object[] { e1, e2 });

        var events = eventStore.Events(streamId).ToArray();

        events[0].Event.ShouldDeepEqual(e1);
        events[1].Event.ShouldDeepEqual(e2);
    }

    [Test]
    public void TimePreservedAccordingToIClock()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        var time = _clock.UtcNow;

        eventStore.Save(streamId, new object[] { e });

        var events = eventStore.Events(streamId);

        Assert.That(events.Single().CreatedUtc, Is.EqualTo(time));
    }

    [Test]
    public void StreamPositionStartsFrom_0_AndIncreasedBy_1_EveryEvent()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        eventStore.Save(streamId, new object[] { e, e, e, e });

        var events = eventStore.Events(streamId).ToArray();

        for (ulong i = 0; i <= 3; i++)
        {
            Assert.That(events[i].StreamPosition, Is.EqualTo(new StreamPosition(i)));
        }
    }

    [Test]
    public void GlobalPositionIncreases()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        eventStore.Save(streamId, new object[] { e });
        eventStore.Save(streamId, new object[] { e });
        eventStore.Save(streamId, new object[] { e });
        eventStore.Save(streamId, new object[] { e });

        var events = eventStore.Events(streamId).ToArray();

        Assert.That(events[0].GlobalPosition, Is.GreaterThanOrEqualTo(GlobalPosition.Start));
        Assert.That(events[1].GlobalPosition, Is.GreaterThan(events[0].GlobalPosition));
        Assert.That(events[2].GlobalPosition, Is.GreaterThan(events[1].GlobalPosition));
        Assert.That(events[3].GlobalPosition, Is.GreaterThan(events[2].GlobalPosition));
    }

    [Test]
    public void GlobalPositionIncreasesInAllStream()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId1 = new StreamId();
        var streamId2 = new StreamId();

        eventStore.Save(streamId1, new object[] { e });
        eventStore.Save(streamId2, new object[] { e });
        eventStore.Save(streamId1, new object[] { e });
        eventStore.Save(streamId2, new object[] { e });

        foreach (var pair in eventStore.AllEvents().Skip(1).Zip(eventStore.AllEvents()))
        {
            Assert.That(pair.First.GlobalPosition, Is.GreaterThan(pair.Second.GlobalPosition));
        }
    }

    [Test]
    public void SaveOnlyPossibleInOrder_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        Assert.DoesNotThrow(() => eventStore.Save(streamId, StreamRevision.NotExists, new object[] { e }));
        Assert.DoesNotThrow(() => eventStore.Save(streamId, 0, new object[] { e }));
        Assert.DoesNotThrow(() => eventStore.Save(streamId, 1, new object[] { e, e }));
        Assert.DoesNotThrow(() => eventStore.Save(streamId, 3, new object[] { e }));
        Assert.DoesNotThrow(() => eventStore.Save(streamId, 4, new object[] { e }));
    }

    [Test]
    public void CantSaveToNonExistingStream_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        var ex = Assert.Throws<EventStoreException>(() => eventStore.Save(streamId, 1234543, new object[] { e }));
        Assert.That(ex!.Message, Is.EqualTo($"Stream not exists! ({streamId})"));
    }

    [Test]
    public void ExistingStreamCantBeCreatedAgain_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();
        eventStore.Save(streamId, StreamRevision.NotExists, new object[] { e });

        var ex = Assert.Throws<EventStoreException>(() =>
            eventStore.Save(streamId, StreamRevision.NotExists, new object[] { e }));
        Assert.That(ex!.Message, Is.EqualTo($"Stream already exists! ({streamId})"));
    }

    [Test]
    public void CantSaveToAlreadyModifiedStream_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var e = TestEvent();
        var streamId = new StreamId();

        eventStore.Save(streamId, StreamRevision.NotExists, new object[] { e, e, e, e, e });

        var ex = Assert.Throws<EventStoreException>(() => eventStore.Save(streamId, 2, new object[] { e }));
        Assert.That(ex!.Message, Is.EqualTo($"Stream modified! ({streamId}, current rev.: 4, wanted rev.: 2)"));
    }

    [Test]
    public void ThousandEvent()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();
        eventStore.Save(
            streamId,
            Enumerable.Repeat(TestEvent(), 1000).ToList());

        Assert.That(eventStore.Events(streamId).Count, Is.EqualTo(1000));
    }

    [Test]
    public void ThousandEvent_WhenUsingConcurrency()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();
        eventStore.Save(
            streamId,
            StreamRevision.NotExists,
            Enumerable.Repeat(TestEvent(), 1000).ToList());

        Assert.That(eventStore.Events(streamId).Count, Is.EqualTo(1000));
    }

    [Test]
    public void SubscribeAndProcessAllEvents()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();
        var lastEventPositionBeforeSave = eventStore
            .AllEvents()
            .DefaultIfEmpty(DefaultEvent())
            .Last()
            .GlobalPosition;

        eventStore.Save(
            streamId,
            Enumerable.Repeat(TestEvent(), 1000).ToList());

        var consumer1 = new TestEventCountingConsumer();
        var consumer2 = new TestEventCountingConsumer();

        var tokenSource = new CancellationTokenSource();

        tokenSource.CancelAfter(300);

        eventStore.SubscribeAll(lastEventPositionBeforeSave, consumer1.Consume, tokenSource.Token);
        eventStore.SubscribeAll(lastEventPositionBeforeSave, consumer2.Consume, tokenSource.Token);

        eventStore.Save(new StreamId(), new[] { new TestEvent("123") });

        Task.Delay(500).Wait();

        Assert.That(consumer1.EventCount, Is.EqualTo(1001));
        Assert.That(consumer2.EventCount, Is.EqualTo(1001));
    }

    private static PersistedEvent DefaultEvent()
    {
        return new PersistedEvent()
        {
            GlobalPosition = GlobalPosition.Start,
            Event = null,
            StreamId = new StreamId(),
            CreatedUtc = DateTime.UtcNow,
            StreamPosition = new StreamPosition(0UL)
        };
    }

    [Test]
    public void StartWithTheRightOneWhenSubscribe()
    {
        var eventStore = EventStore();
        var streamId = new StreamId();

        var lastEventPositionBeforeSave1 = eventStore
            .AllEvents()
            .DefaultIfEmpty(DefaultEvent())
            .Last()
            .GlobalPosition;

        eventStore.Save(
            streamId,
            new List<object>()
            {
                new TestEvent("check 12345"),
                new TestEvent("check 67890"),
            });

        var lastEventPositionBeforeSave2 = eventStore
            .AllEvents()
            .DefaultIfEmpty(DefaultEvent())
            .Last()
            .GlobalPosition;

        eventStore.Save(
            streamId,
            new List<object>()
            {
                new TestEvent("check apple"),
                new TestEvent("check banana"),
            });

        var consumer1 = new TestFirstEventConsumer();
        var consumer2 = new TestFirstEventConsumer();

        var tokenSource = new CancellationTokenSource();

        tokenSource.CancelAfter(200);

        eventStore.SubscribeAll(
            new GlobalPosition(lastEventPositionBeforeSave1),
            consumer1.Consume,
            tokenSource.Token);
        eventStore.SubscribeAll(
            new GlobalPosition(lastEventPositionBeforeSave2),
            consumer2.Consume,
            tokenSource.Token);

        Task.Delay(300).Wait();

        Assert.That(consumer1.FirstGlobalPosition, Is.GreaterThan(new GlobalPosition(lastEventPositionBeforeSave1)));
        Assert.That(consumer1.FirstComment, Is.EqualTo("check 12345"));
        Assert.That(consumer2.FirstGlobalPosition, Is.GreaterThan(new GlobalPosition(lastEventPositionBeforeSave2)));
        Assert.That(consumer2.FirstComment, Is.EqualTo("check apple"));
    }

    private class TestEventCountingConsumer
    {
        public ulong EventCount { get; private set; }

        public void Consume(PersistedEvent e)
        {
            EventCount++;
        }
    }

    private class TestFirstEventConsumer
    {
        public GlobalPosition? FirstGlobalPosition { get; private set; }
        public string? FirstComment { get; private set; }

        public void Consume(PersistedEvent e)
        {
            switch (e.Event)
            {
                case TestEvent @event:
                    if (FirstGlobalPosition.HasValue)
                        return;
                    FirstGlobalPosition = e.GlobalPosition;
                    FirstComment = @event.Comment;
                    break;
            }
        }
    }
}

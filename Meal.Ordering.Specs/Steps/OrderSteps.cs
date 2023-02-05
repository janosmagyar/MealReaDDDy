using EventStore.Api;
using Meal.Ordering.Api;
using Meal.Ordering.Specs.Support;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Meal.Ordering.Specs.Steps;

[Binding]
public class OrderSteps
{
    private readonly ExceptionContext _exceptionContext;
    private readonly IEventStore _eventStore;
    private readonly string _id = Guid.NewGuid().ToString("N");
    private IOrderNumberGenerator _orderNumberGenerator;
    private OrderMealCommand? _orderMealCommand;
    private MealProjectedState _startState = new();
    private Meal CreateNewMealFromStartState() => new(_eventStore, _id); // new(_eventStore, _id, new(_startState));

    public OrderSteps(
        ExceptionContext exceptionContext,
        IEventStore eventStore,
        IOrderNumberGenerator orderNumberGenerator
        )
    {
        _exceptionContext = exceptionContext;
        _eventStore = eventStore;
        _orderNumberGenerator = orderNumberGenerator;
    }

    [Given(@"the order to take away")]
    public void GivenTheOrder(FoodItem[] items)
    {
        _orderMealCommand = new OrderMealCommand(items, Serving.paperbag, null);
    }

    [Given(@"the order serve to table '([^']*)'")]
    public void GivenTheOrderServeToTable(int table, FoodItem[] items)
    {
        _orderMealCommand = new OrderMealCommand(items, Serving.tray, table);
    }

    [Given(@"the order to pick up at counter")]
    public void GivenTheOrderToPickUpAtCounter(FoodItem[] items)
    {
        _orderMealCommand = new OrderMealCommand(items, Serving.tray, null);
    }
    [Given(@"the order to take-away and having a table number with items")]
    public void GivenTheOrderToTake_AwayAndHavingATableNumberWithItems(FoodItem[] items)
    {
        _orderMealCommand = new OrderMealCommand(items, Serving.paperbag, 99);
    }

    [Given(@"the next order number from the central order service will be '([^']*)'")]
    public void GivenTheNextOrderNumberIs(OrderNumber orderNumber)
    {
        _orderNumberGenerator = new FixedOrderNumberGenerator(orderNumber);
    }

    [Given(@"an order")]
    public void GivenAnOrder(MealProjectedState startState)
    {
        _startState = startState;
    }

    [Given(@"the items in the order are")]
    public void GivenTheItemsInTheOrderAre(TrackedItem[] items)
    {
        _startState.Items = items;
    }

    [When(@"no payment happened")]
    public void WhenNoPaymentHappened()
    {
        //nop
    }

    [When(@"payment failed")]
    public void WhenPaymentFailed()
    {
        _exceptionContext.SaveExceptionIfError(() => CreateNewMealFromStartState().PaymentFailed());
    }

    [When(@"payment succeeded")]
    public void WhenPaymentSucceeded()
    {
        _exceptionContext.SaveExceptionIfError(() => CreateNewMealFromStartState().PaymentSucceeded());
    }

    [When(@"expeditor confirms that one piece from '([^']*)' item is prepared")]
    public void WhenExpeditorConfirmsThatOnePieceFromItemIsPrepared(ItemIndex itemIndex)
    {
        _exceptionContext.SaveExceptionIfError(() =>
            CreateNewMealFromStartState().ConfirmItemPrepared(new ConfirmMealItemPreparedCommand(itemIndex)));
    }

    [When(@"the customer place the order")]
    public void WhenTheCustomerPlaceTheOrder()
    {
        _exceptionContext.SaveExceptionIfError(() =>
            new Meal(_eventStore, _id)
                .Order(_orderMealCommand!, _orderNumberGenerator!));
    }

    [When(@"customer takes the order")]
    public void WhenCustomerTakesTheOrder()
    {
        _exceptionContext.SaveExceptionIfError(() =>
            CreateNewMealFromStartState().TakeAway());
    }

    [When(@"customer pickes up the order")]
    public void WhenCustomerPickesUpTheOrder()
    {
        _exceptionContext.SaveExceptionIfError(() =>
            CreateNewMealFromStartState().PickUp());
    }

    [When(@"expeditor serves the meal to the table")]
    public void WhenExpeditorServesTheMealToTheTable()
    {
        _exceptionContext.SaveExceptionIfError(() =>
            CreateNewMealFromStartState().ServeToTable());
    }

    [Then(@"then the order payment is '([^']*)'")]
    public void ThenThenTheOrderPaymentIs(PaymentState expectedState)
    {
        var meal = CreateNewMealFromStartState();
        Assert.That(meal.MealPublicState.Payment, Is.EqualTo(expectedState));
    }

    [Then(@"the order")]
    private void ThenTheOrder(PartialMealState s)
    {
        var meal = CreateNewMealFromStartState();
        Assert.That(meal.MealPublicState.State, Is.EqualTo(s.State));
        Assert.That(meal.MealPublicState.Table, Is.EqualTo(s.Table));
        Assert.That(meal.MealPublicState.Serving, Is.EqualTo(s.Serving));
        Assert.That(meal.MealPublicState.OrderNumber, Is.EqualTo(s.OrderNumber));
    }

    [Then(@"the items in the order are")]
    public void ThenTheItemsInTheOrderAre(OrderedItem[] expectedItems)
    {
        var meal = CreateNewMealFromStartState();
        Assert.That(meal.MealPublicState.Items, Is.EquivalentTo(expectedItems));
    }

    [StepArgumentTransformation]
    public FoodItem[] FoodItems(Table table)
    {
        return table.Rows.Select(r =>
            new FoodItem(
                int.Parse(r["Count"]),
                r["Name"],
                Enum.Parse<Category>(r["Category"])
            )).ToArray();
    }

    [StepArgumentTransformation]
    public OrderedItem[] OrderedItems(Table table)
    {
        return table.Rows.Select(r =>
            new OrderedItem(
                int.Parse(r["Count"]),
                r["Name"],
                Enum.Parse<Category>(r["Category"]),
                bool.Parse(r["Prepared"])
                )).ToArray();
    }

    [StepArgumentTransformation]
    public TrackedItem[] TrackedItems(Table table)
    {
        return table.Rows.Select(r =>
            new TrackedItem(
                int.Parse(r["Count"]),
                r["Name"],
                Enum.Parse<Category>(r["Category"]),
                int.Parse(r["Prepared"])
            )).ToArray();
    }

    [StepArgumentTransformation]
    public OrderNumber OrderNumber(string value) => new(int.Parse(value));

    [StepArgumentTransformation]
    public ItemIndex ItemIndex(string value) => new(int.Parse(value.Substring(0,value.Length-2))-1);

    [StepArgumentTransformation]
    private PartialMealState PartialMealStateTransformation(Table table) => table.CreateInstance<PartialMealState>();

    [StepArgumentTransformation]
    public MealProjectedState InternalState(Table table) => table.CreateInstance<MealProjectedState>();

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Service.Instance.ValueRetrievers.Register(new TableNumberRetriever());
        Service.Instance.ValueRetrievers.Register(new OrderNumberRetriever());
    }

    private sealed class PartialMealState
    {
        public required OrderState State { get; init; }
        public required OrderNumber OrderNumber { get; init; }
        public required Serving Serving { get; init; }
        public required TableNumber Table { get; init; }
    }
}

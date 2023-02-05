using Meal.Ordering.Api;

namespace Meal.Ordering;

public class MealProjectedState
{
    public TrackedItem[] Items { get; set; } = Array.Empty<TrackedItem>();
    public OrderNumber OrderNumber { get; set; }
    public Serving Serving { get; set; }
    public TableNumber Table { get; set; }
    public OrderState State { get; set; }
    public PaymentState Payment { get; set; }

    public MealProjectedState()
    {
    }

    public MealProjectedState(MealProjectedState st)
    {
        Items = st.Items.Select(i=> new TrackedItem(i)).ToArray();
        OrderNumber = st.OrderNumber;
        Serving = st.Serving;
        Table = st.Table;
        State = st.State;
        Payment = st.Payment;
    }
}
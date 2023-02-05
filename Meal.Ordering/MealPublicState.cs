using Meal.Ordering.Api;

namespace Meal.Ordering;

public record MealPublicState
{
    public OrderState State { get; init; }
    public OrderNumber OrderNumber { get; init; }
    public Serving Serving { get; init; }
    public TableNumber Table { get; init; }
    public OrderedItem[] Items { get; init; }
    public PaymentState Payment { get; init; }
}

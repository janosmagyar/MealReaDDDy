using Meal.Ordering.Api;

namespace Meal.Ordering.Specs.Support;

public class FixedOrderNumberGenerator : IOrderNumberGenerator
{
    private readonly OrderNumber _orderNumber;

    public FixedOrderNumberGenerator(OrderNumber orderNumber)
    {
        _orderNumber = orderNumber;
    }

    public OrderNumber GetNext() => _orderNumber;
}
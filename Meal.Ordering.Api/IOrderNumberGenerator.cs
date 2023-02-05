namespace Meal.Ordering.Api;

public interface IOrderNumberGenerator
{
    public OrderNumber GetNext();
}
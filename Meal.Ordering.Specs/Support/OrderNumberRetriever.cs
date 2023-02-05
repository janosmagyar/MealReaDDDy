using Meal.Ordering.Api;
using TechTalk.SpecFlow.Assist.ValueRetrievers;

namespace Meal.Ordering.Specs.Support;

public class OrderNumberRetriever : ClassRetriever<OrderNumber>
{
    protected override OrderNumber GetNonEmptyValue(string value) => new(int.Parse(value));
}
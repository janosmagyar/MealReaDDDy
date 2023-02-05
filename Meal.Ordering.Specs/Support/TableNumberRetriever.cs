using Meal.Ordering.Api;
using TechTalk.SpecFlow.Assist.ValueRetrievers;

namespace Meal.Ordering.Specs.Support;

public class TableNumberRetriever : StructRetriever<TableNumber>
{
    protected override TableNumber GetNonEmptyValue(string value)
    {
        return value == "empty" ? new TableNumber() : new TableNumber(int.Parse(value));
    }
}
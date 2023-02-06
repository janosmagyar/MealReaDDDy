using EventStore.Api;

namespace Meal.Events
{
    public record MealOrdered : Event
    {
        public required int OrderNumber { get; init; }
        public required int? Table { get; init; }
        public required string Serving { get; init; }
        public required Item[] Items { get; init; }
    }

    public record Item
    {
        public required int Count { get; init; }
        public required string Name { get; init; }
        public required string Category { get; init; }
    }
}

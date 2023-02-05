namespace Meal.Ordering.Api;

public record FoodItem
{
    public int Count { get; init; }
    public string Name { get; init; }
    public Category Category { get; init; }

    public FoodItem(int count, string name, Category category)
    {
        if (count is < 1 or > 20)
            throw new ArgumentException("Invalid food item!", nameof(count));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Invalid food item!", nameof(name));
        Count = count;
        Name = name;
        Category = category;
    }
};

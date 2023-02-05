using Meal.Ordering.Api;

namespace Meal.Ordering;

public class TrackedItem
{
    public int Count { get;  }
    public string Name { get;  }
    public Category Category { get;  }
    public bool IsPrepared => Count == _preparedCount;
    public bool IsOneMissing => Count == _preparedCount + 1;

    private int _preparedCount = 0;

    public TrackedItem(int count, string name, Category category)
    {
        Count = count;
        Name = name;
        Category = category;
    }

    public TrackedItem(int count, string name, Category category, int prepared)
    {
        Count = count;
        Name = name;
        Category = category;
        _preparedCount = prepared;
    }

    public TrackedItem(TrackedItem item)
    {
        Count = item.Count;
        Name = item.Name;
        Category = item.Category;
        _preparedCount = item._preparedCount;
    }

    public void PrepareOne()
    {
        if (_preparedCount < Count)
            _preparedCount++;
        else
            throw new InvalidOperationException("Everything prepared already!");
    }
}
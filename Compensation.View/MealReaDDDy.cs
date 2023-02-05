namespace Compensation.View;

public class MealReaDDDy
{
    public string Id { get; set; }
    public TimeSpan Duration => End - Start;
    public bool IsReaDDDy => End != DateTime.MaxValue;
    public DateTime Start { get; set; }
    public DateTime End { get; set; } = DateTime.MaxValue;
}
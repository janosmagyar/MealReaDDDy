namespace Meal.Ordering.Specs.Steps;

public class ExceptionContext
{
    public Exception? LastException { get; set; }

    public void SaveExceptionIfError(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            LastException = e;
        }
    }
}